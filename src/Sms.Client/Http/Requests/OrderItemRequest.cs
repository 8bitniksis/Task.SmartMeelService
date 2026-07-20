using System.Globalization;
using System.Text.Json.Serialization;
using Sms.Client.Models;

namespace Sms.Client.Http.Requests;

public sealed class OrderItemRequest
{
    [JsonPropertyName("Id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("Quantity")]
    public string Quantity { get; init; } = string.Empty;

    public static OrderItemRequest From(OrderItem item)
    {
        return new()
        {
            Id = item.Id,
            Quantity = item.Quantity.ToString(
                CultureInfo.InvariantCulture)
        };
    }
}