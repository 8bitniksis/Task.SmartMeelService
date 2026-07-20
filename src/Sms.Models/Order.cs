using System.Text.Json.Serialization;

namespace Sms.Models
{
    /// <summary>
    /// Represents an order containing multiple items.
    /// </summary>
    public class Order
    {
        /// <summary>
        /// Gets or sets the unique identifier of the order.
        /// </summary>
        [JsonPropertyName("OrderId")]
        public string OrderId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the list of items included in the order.
        /// </summary>
        [JsonPropertyName("MenuItems")] // Matches the JSON field name in the example request
        public List<OrderItem> Items { get; set; } = new();
    }
}