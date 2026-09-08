namespace RagEngine.Infrastructure.Config
{
    public class OllamaOptions
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string EmbeddingModel { get; set; } = string.Empty;
        public string EmbeddingEndpoint { get; set; } = string.Empty;
        public int EmbeddingDimensions { get; set; } = 0;
        public string GenerationModel { get; set; } = string.Empty;
        public string GenerationEndpoint { get; set; } = string.Empty;
        public double GenerationTemperature { get; set; } = 0.1;
        public int GenerationMaxTokens { get; set; } = 200;
        public string GenerationKeepAlive { get; set; } = "30m";
        public bool GenerationThink { get; set; } = false;

    }
}
