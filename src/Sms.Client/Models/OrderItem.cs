namespace Sms.Client.Models;

public sealed class OrderItem
{
    public required string Id { get; init; }

    public decimal Quantity { get; init; }
}