namespace RagEngine.Infrastructure.Embedding
{
    public class OllamaEmbedRequest
    {
        public string Model { get; set; } = string.Empty;
        public string[] Input { get; set; } = Array.Empty<string>();
    }
}
