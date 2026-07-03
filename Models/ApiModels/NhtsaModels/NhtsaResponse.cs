using System.Text.Json.Serialization;

namespace GearFix.Models.ApiModels.NhtsaModels
{
    public class NhtsaResponse
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }
        [JsonPropertyName("results")]
        public List<NhtsaResult> Results { get; set; } = new();
    }

    public class NhtsaResult
    {
        [JsonPropertyName("vin")]
        public string Vin { get; set; } = string.Empty;
        [JsonPropertyName("components")]
        public string Components { get; set; } = string.Empty;
        [JsonPropertyName("summary")]
        public string Summary { get; set; } = string.Empty;
        [JsonPropertyName("products")]
        public List<Product> Products { get; set; } = new();
    }

    public class Product
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
        [JsonPropertyName("productYear")]
        public string ProductYear { get; set; } = string.Empty;
        [JsonPropertyName("productMake")]
        public string ProductMake { get; set; } = string.Empty;
        [JsonPropertyName("productModel")]
        public string ProductModel { get; set;  } = string.Empty;
        [JsonPropertyName("manufacturer")]
        public string Manufacturer { get; set;  } = string.Empty;
    }
}
