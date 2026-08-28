namespace RagEngine.Infrastructure.Ollama.Synthesis
{
    public class GenerateRequest
    {
        public string Model { get; set; } = string.Empty;
        public string Prompt { get; set; } = string.Empty;
        public bool Stream { get; init; } = false;
    }
}
