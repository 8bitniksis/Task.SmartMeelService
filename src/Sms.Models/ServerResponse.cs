using System.Text.Json.Serialization;

namespace Sms.Models
{
    /// <summary>
    /// Generic response wrapper from the server.
    /// </summary>
    public class ServerResponse<T>
    {
        [JsonPropertyName("Command")]
        public string Command { get; set; } = string.Empty;

        [JsonPropertyName("Success")]
        public bool Success { get; set; }

        [JsonPropertyName("ErrorMessage")]
        public string? ErrorMessage { get; set; }

        [JsonPropertyName("Data")]
        public T? Data { get; set; }
    }

    /// <summary>
    /// Specific response for GetMenu command.
    /// </summary>
    public class GetMenuResponse : ServerResponse<MenuData>
    {
    }

    /// <summary>
    /// Data payload for GetMenu response.
    /// </summary>
    public class MenuData
    {
        [JsonPropertyName("MenuItems")]
        public List<Dish> MenuItems { get; set; } = new();
    }

    /// <summary>
    /// Specific response for SendOrder command.
    /// </summary>
    public class SendOrderResponse
    {
        [JsonPropertyName("Command")]
        public string Command { get; set; } = string.Empty;

        [JsonPropertyName("Success")]
        public bool Success { get; set; }

        [JsonPropertyName("ErrorMessage")]
        public string? ErrorMessage { get; set; }
    }
}