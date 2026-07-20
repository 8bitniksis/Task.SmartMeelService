namespace Sms.Client.Models;

public sealed class Order
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public List<OrderItem> Items { get; } = [];
}