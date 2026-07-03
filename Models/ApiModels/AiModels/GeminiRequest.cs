using System.Text.Json.Serialization;

namespace GearFix.Models.ApiModels.AiModels
{
    public class GeminiRequest
    {
        [JsonPropertyName("contents")]
        public List<Message> Contents { get; set; } = new();

        [JsonPropertyName("system_instruction")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public SystemInstruction SystemInstruction { get; set; } = new();
    }

    public class Message
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;
        [JsonPropertyName("parts")]
        public List<Part> Parts { get; set; } = new();
    }

    public class SystemInstruction
    {
        [JsonPropertyName("parts")]
        public List<Part> Parts { get; set; } = new();
    }

    public class Part
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }
}
