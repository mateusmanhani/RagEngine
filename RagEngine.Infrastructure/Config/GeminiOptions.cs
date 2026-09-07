namespace RagEngine.Infrastructure.Config
{
    public class GeminiOptions
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public string EmbeddingModel { get; set; } = string.Empty;
        public int EmbeddingDimensions { get; set; }
        public string ApiKey { get; set; } = string.Empty;
    }
}
