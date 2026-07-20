using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Sms.Models;

namespace Sms.HttpClientLibrary
{
    /// <summary>
    /// Provides methods to interact with the server via HTTP requests.
    /// </summary>
    public class HttpApiClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _username;
        private readonly string _password;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpApiClient"/> class.
        /// </summary>
        /// <param name="baseAddress">The base address of the server endpoint.</param>
        /// <param name="username">The username for Basic authentication.</param>
        /// <param name="password">The password for Basic authentication.</param>
        public HttpApiClient(string baseAddress, string username, string password)
        {
            var handler = new HttpClientHandler();
            // NOTE: In production, do not disable certificate validation.
            // handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            _httpClient = new HttpClient(handler);
            _httpClient.BaseAddress = new Uri(baseAddress);

            _username = username ?? throw new ArgumentNullException(nameof(username));
            _password = password ?? throw new ArgumentNullException(nameof(password));

            // Set up Basic Authentication header once
            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_username}:{_password}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }

        /// <summary>
        /// Sends a request to retrieve the menu from the server.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A list of <see cref="Dish"/> objects if successful, otherwise throws an exception.</returns>
        /// <exception cref="HttpRequestException">Thrown when the request fails or the server returns an error.</exception>
        public async Task<List<Dish>> GetMenuAsync(CancellationToken cancellationToken = default)
        {
            var requestContent = new
            {
                Command = "GetMenu",
                CommandParameters = new { WithPrice = true }
            };

            var jsonPayload = JsonSerializer.Serialize(requestContent, _jsonSerializerOptions);
            var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("", httpContent, cancellationToken); // Uses base address

            // Note: The spec says status code is always 200, so we rely on Success field in body.
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var serverResponse = JsonSerializer.Deserialize<GetMenuResponse>(responseContent, _jsonSerializerOptions);

            if (serverResponse?.Success != true)
            {
                var errorMessage = serverResponse?.ErrorMessage ?? "Unknown error occurred while fetching menu.";
                throw new HttpRequestException($"Server responded with error: {errorMessage}");
            }

            // Return empty list if no menu items found, though unlikely if Success=true
            return serverResponse.Data?.MenuItems ?? new List<Dish>();
        }


        /// <summary>
        /// Sends an order to the server.
        /// </summary>
        /// <param name="order">The <see cref="Order"/> object to send.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>True if the server indicates success, otherwise throws an exception.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the order is null.</exception>
        /// <exception cref="HttpRequestException">Thrown when the request fails or the server returns an error.</exception>
        public async Task<bool> SendOrderAsync(Order order, CancellationToken cancellationToken = default)
        {
            if (order == null)
            {
                throw new ArgumentNullException(nameof(order));
            }

            var requestContent = new
            {
                Command = "SendOrder",
                CommandParameters = order // The Order object itself matches the CommandParameters structure
            };

            var jsonPayload = JsonSerializer.Serialize(requestContent, _jsonSerializerOptions);
            var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("", httpContent, cancellationToken); // Uses base address

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var serverResponse = JsonSerializer.Deserialize<SendOrderResponse>(responseContent, _jsonSerializerOptions);

            if (serverResponse?.Success != true)
            {
                var errorMessage = serverResponse?.ErrorMessage ?? "Unknown error occurred while sending order.";
                throw new HttpRequestException($"Server responded with error: {errorMessage}");
            }

            return true; // Success field was true
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}