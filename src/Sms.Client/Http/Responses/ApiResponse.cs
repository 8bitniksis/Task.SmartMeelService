using System.Text.Json.Serialization;

namespace Sms.Client.Http.Responses;

public sealed class ApiResponse<T>
{
    [JsonPropertyName("Success")]
    public bool Success { get; init; }

    [JsonPropertyName("Data")]
    public T? Data { get; init; }

    [JsonPropertyName("ErrorMessage")]
    public string? ErrorMessage { get; init; }
}