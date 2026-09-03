using RagEngine.Infrastructure.Config;
using System.Text.Json.Serialization;

namespace RagEngine.Infrastructure.Synthesis
{
    public class OllamaGenerateRequest
    {
        public string Model { get; set; } = string.Empty;
        public string Prompt { get; set; } = string.Empty;
        public bool? Stream { get; init; } = false;

        [JsonPropertyName("keep_alive")]
        public string? KeepAlive { get; init; } = "30m";
        public bool? Think { get; init; }
        public GenerateOptions? Options { get; init; } = new();
    }
}
