namespace RagEngine.Infrastructure.Config
{
    public class GeminiOptions
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public string EmbeddingModel { get; set; } = string.Empty;
        public int EmbeddingDimenstions { get; set; }
        public string ApiKey { get; set; } = string.Empty;
    }
}
