namespace RagEngine.Infrastructure.Embedding
{
    public class OllamaOptions
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string EmbeddingModel { get; set; } = string.Empty;
        public string EmbeddingEndpoint { get; set; } = string.Empty;
        public int EmbeddingDimensions { get; set; } = 0;

    }
}
