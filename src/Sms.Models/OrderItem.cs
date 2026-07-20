using System.Text.Json.Serialization;

namespace Sms.Models
{
    /// <summary>
    /// Represents an item within an order.
    /// </summary>
    public class OrderItem
    {
        /// <summary>
        /// Gets or sets the unique identifier of the dish being ordered.
        /// </summary>
        [JsonPropertyName("Id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the quantity of the dish being ordered.
        /// For weighted items, this can be a decimal value (e.g., 0.408 kg).
        /// For non-weighted items, this should be an integer.
        /// </summary>
        [JsonPropertyName("Quantity")]
        public double Quantity { get; set; }
    }
}