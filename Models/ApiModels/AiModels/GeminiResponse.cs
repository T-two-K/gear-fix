using System.Text.Json.Serialization;

namespace GearFix.Models.ApiModels.AiModels
{
    public class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public List<Candidate> Candidates { get; set; } = new();
    }

    public class Candidate
    {
        [JsonPropertyName("content")]
        public ContentResponse Content { get; set; } = new();
    }

    public class ContentResponse
    {
        [JsonPropertyName("parts")]
        public List<Part> Parts { get; set; } = new();
    }
}
