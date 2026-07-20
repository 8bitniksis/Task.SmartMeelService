using System.Text.Json.Serialization;

namespace Sms.Models
{
    /// <summary>
    /// Represents a dish item from the menu.
    /// </summary>
    public class Dish
    {
        /// <summary>
        /// Gets or sets the unique identifier of the dish.
        /// </summary>
        [JsonPropertyName("Id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the article number (SKU) of the dish.
        /// </summary>
        [JsonPropertyName("Article")]
        public string Article { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the dish.
        /// </summary>
        [JsonPropertyName("Name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the price per unit of the dish.
        /// </summary>
        [JsonPropertyName("Price")]
        public double Price { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the dish is sold by weight.
        /// </summary>
        [JsonPropertyName("IsWeighted")]
        public bool IsWeighted { get; set; }

        /// <summary>
        /// Gets or sets the full path category of the dish (e.g., PRODUCTION\Side dishes).
        /// </summary>
        [JsonPropertyName("FullPath")]
        public string FullPath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the list of barcodes associated with the dish.
        /// </summary>
        [JsonPropertyName("Barcodes")]
        public List<string> Barcodes { get; set; } = new();
    }
}