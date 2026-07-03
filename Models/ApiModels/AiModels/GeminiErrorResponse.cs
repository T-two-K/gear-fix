using System.Text.Json.Serialization;

namespace GearFix.Models.ApiModels.AiModels
{
    public class GeminiErrorResponse
    {
        [JsonPropertyName("error")]
        public GeminiError Error { get; set; } = new();
    }

    public class GeminiError
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }
        [JsonPropertyName ("message")]
        public string Message { get; set; } = string.Empty;
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
    }
}
