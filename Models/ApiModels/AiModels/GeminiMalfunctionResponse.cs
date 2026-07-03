using System.Text.Json.Serialization;

namespace GearFix.Models.ApiModels.AiModels
{
    public class GeminiMalfunctionResponse 
    {
        [JsonPropertyName("malfunctions")]
        public List<Malfunction> Malfunctions { get; set; } = new();
        [JsonPropertyName("needClarification")]
        public bool NeedClarification { get; set; } = false;
        [JsonPropertyName("additionalQuestion")]
        public string? AdditionalQuestion { get; set; }
    }

    public class Malfunction
    {
        [JsonPropertyName("probability")]
        public int Probability { get; set; }
        [JsonPropertyName("danger")]
        public int Danger { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
        [JsonPropertyName("malfunctionDescription")]
        public string MalfunctionDescription { get; set; } = string.Empty;
        [JsonPropertyName("explanation")]
        public string Explanation { get; set; } = string.Empty;
        [JsonPropertyName("rubric")]
        public string Rubric { get; set; } = string.Empty;
    }
}
