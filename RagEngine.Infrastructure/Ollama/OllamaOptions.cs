namespace RagEngine.Infrastructure.Ollama
{
    public class OllamaOptions
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string EmbeddingModel { get; set; } = string.Empty;
        public string EmbeddingEndpoint { get; set; } = string.Empty;
        public int EmbeddingDimensions { get; set; } = 0;
        public string GenerationModel { get; set; } = string.Empty;
        public string GenerationEndpoint { get; set; } = string.Empty;

    }
}
