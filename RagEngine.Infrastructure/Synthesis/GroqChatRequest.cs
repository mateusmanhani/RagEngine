using System.Text.Json.Serialization;

namespace RagEngine.Infrastructure.Synthesis
{
    public class GroqChatRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;
        
        [JsonPropertyName("messages")]
        public List<GroqMessage> Messages { get; set; } = new();

        [JsonPropertyName("temperature")]
        public double Temperature { get; set; } = 0.2;

        [JsonPropertyName("max_completion_tokens")]
        public int MaxCompletionTokens { get; set; } = 800;
    }

    public class GroqMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }
}
