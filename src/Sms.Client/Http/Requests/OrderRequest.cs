using System.Text.Json.Serialization;

namespace Sms.Client.Http.Requests;

public sealed class OrderRequest
{
    [JsonPropertyName("Id")]
    public Guid Id { get; init; }

    [JsonPropertyName("Items")]
    public List<OrderItemRequest> Items { get; init; } = [];
}