using System.Text.Json.Serialization;

namespace Sms.Client.Http.Responses;

public sealed class MenuItemResponse
{
    [JsonPropertyName("Id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("Article")]
    public string Article { get; init; } = string.Empty;

    [JsonPropertyName("Name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("Price")]
    public decimal Price { get; init; }

    [JsonPropertyName("IsWeighted")]
    public bool IsWeighted { get; init; }

    [JsonPropertyName("FullPath")]
    public string FullPath { get; init; } = string.Empty;

    [JsonPropertyName("Barcodes")]
    public List<string> Barcodes { get; init; } = [];
}