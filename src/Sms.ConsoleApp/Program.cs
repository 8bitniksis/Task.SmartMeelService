using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using Sms.HttpClientLibrary;
using Sms.Models;
using System.Globalization;
using System.Text.RegularExpressions;

// Configure Serilog early
var logFileName = $"test-sms-console-app-{DateTime.UtcNow:yyyyMMdd}.log";
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console() // Log to console
    .WriteTo.File(logFileName, rollingInterval: RollingInterval.Day) // Log to file
    .CreateLogger();

try
{
    Log.Information("Application starting...");

    // Load configuration
    var configuration = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();

    var connectionString = configuration.GetConnectionString("DefaultConnection");
    var baseUrl = configuration["ApiSettings:BaseUrl"];
    var username = configuration["ApiSettings:Username"];
    var password = configuration["ApiSettings:Password"];

    if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
    {
        Log.Fatal("Missing required configuration settings.");
        Console.WriteLine("Missing required configuration settings. Check appsettings.json.");
        return;
    }

    // Initialize Database Context
    var context = new DishDbContext(connectionString);
    await context.Database.EnsureCreatedAsync(); // Creates DB and Table if they don't exist

    Log.Information("Database initialized.");

    // Use HTTP Client Library
    using var httpClient = new HttpApiClient(baseUrl, username, password);

    // Step 2: Get Menu
    Log.Information("Fetching menu...");
    List<Dish> dishes;
    try
    {
        dishes = await httpClient.GetMenuAsync();
        Log.Information("Menu fetched successfully. Count: {Count}", dishes.Count);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to fetch menu: {Message}", ex.Message);
        Console.WriteLine($"Error fetching menu: {ex.Message}");
        return; // Stop execution on failure
    }

    // Step 3: Save to DB and Print
    Log.Information("Saving menu to database...");
    await context.Dishes.AddRangeAsync(dishes);
    await context.SaveChangesAsync(); // Batch save for efficiency
    Log.Information("Menu saved to database.");

    Console.WriteLine("\n--- Menu ---");
    foreach (var dish in dishes)
    {
        Console.WriteLine($"{dish.Name} – {dish.Article} – {dish.Price:F2}"); // Format price to 2 decimal places
    }

    // Step 4 & 5: Create Order Instance and Input
    var order = new Order { OrderId = Guid.NewGuid().ToString() }; // Generate unique order ID
    List<(string ArticleCode, double Quantity)> inputItems = new();

    Console.WriteLine("\n--- Enter Order ---");
    Console.WriteLine("Enter items in format: Code1:Quantity1;Code2:Quantity2;... (e.g., A1004292:1;A1004293:0.408)");
    string inputLine;
    while (true)
    {
        Console.Write("Input: ");
        inputLine = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(inputLine))
        {
            Console.WriteLine("Input cannot be empty. Please try again.");
            continue;
        }

        // Parse input: Code1:Qty1;Code2:Qty2;...
        // Regex to split pairs and then split code/qty within each pair
        var pairs = inputLine.Split(';');
        bool isValidInput = true;
        inputItems.Clear(); // Clear previous attempt

        foreach (var pairStr in pairs)
        {
            var parts = pairStr.Split(':', 2); // Split only on first ':'
            if (parts.Length != 2)
            {
                Console.WriteLine($"Invalid format in '{pairStr}'. Expected 'Code:Quantity'.");
                isValidInput = false;
                break;
            }

            string code = parts[0].Trim();
            string qtyStr = parts[1].Trim();

            if (!double.TryParse(qtyStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double quantity) || quantity <= 0)
            {
                Console.WriteLine($"Invalid quantity '{qtyStr}' for code '{code}'. Quantity must be a positive number.");
                isValidInput = false;
                break;
            }

            inputItems.Add((code, quantity));
        }

        if (!isValidInput)
        {
            continue; // Prompt for input again
        }

        // Step 6: Validate codes exist and quantities are positive
        bool allCodesValid = true;
        var availableArticles = dishes.Select(d => d.Article).ToHashSet();
        foreach (var (articleCode, quantity) in inputItems)
        {
            if (!availableArticles.Contains(articleCode))
            {
                Console.WriteLine($"Invalid code '{articleCode}'. Not found in the menu.");
                allCodesValid = false;
                break;
            }
            // Quantity check already done during parsing
        }

        if (allCodesValid)
        {
            break; // Exit loop if all codes are valid
        }
    }

    // Step 7: Add validated items to order and send
    Log.Information("Building order from user input...");
    var dishLookup = dishes.ToDictionary(d => d.Article, d => d.Id); // Map Article -> Id
    foreach (var (articleCode, quantity) in inputItems)
    {
        order.Items.Add(new OrderItem { Id = dishLookup[articleCode], Quantity = quantity });
    }

    Log.Information("Sending order {OrderId} to server...", order.OrderId);
    try
    {
        bool sendResult = await httpClient.SendOrderAsync(order);
        if (sendResult)
        {
            Log.Information("Order {OrderId} sent successfully.", order.OrderId);
            Console.WriteLine("\n--- Result ---");
            Console.WriteLine("УСПЕХ"); // Success
        }
        else
        {
             // This case should theoretically not happen if SendOrderAsync throws on failure,
             // but added for completeness if logic changes.
            Log.Warning("Order {OrderId} send indicated failure internally.", order.OrderId);
            Console.WriteLine("\n--- Result ---");
            Console.WriteLine("Неизвестная внутренняя ошибка при отправке.");
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to send order {OrderId}: {Message}", order.OrderId, ex.Message);
        Console.WriteLine($"\n--- Result ---");
        Console.WriteLine(ex.Message); // Print server error message or generic error
    }
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly: {Message}", ex.Message);
}
finally
{
    Log.CloseAndFlush(); // Ensure logs are flushed before exit
    Console.WriteLine("\nPress any key to exit...");
    Console.ReadKey();
}

// --- Entity Framework Core Setup ---
public class DishDbContext : DbContext
{
    private readonly string _connectionString;

    public DishDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    public DbSet<DishEntity> Dishes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(_connectionString); // Use Npgsql for PostgreSQL
    }

     // Use an entity that maps to the Dish model for EF Core
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DishEntity>(entity =>
        {
            entity.HasKey(e => e.Id); // Assuming Id from Dish is primary key
            entity.Property(e => e.Id).IsRequired();
            entity.Property(e => e.Article).IsRequired().HasMaxLength(100); // Example length
            entity.Property(e => e.Name).IsRequired().HasMaxLength(500); // Example length
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)"); // Example precision
            // Configure other properties as needed
        });
    }
}

// EF Core entity mapping Dish model
public class DishEntity
{
    public string Id { get; set; } = string.Empty;
    public string Article { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public double Price { get; set; }
    public bool IsWeighted { get; set; }
    public string FullPath { get; set; } = string.Empty;
    // Note: Collections like Barcodes might need special handling (e.g., JSON column or separate table)
    // For simplicity here, we might store them as a JSON string if needed, or map differently.
    // Let's assume Barcodes are not stored in DB for this example, just fetched/displayed.
}