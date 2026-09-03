namespace RagEngine.Infrastructure.Config
{
    public class GroqOptions
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public double Temperature { get; set; } = 0.2;
        public int MaxCompletionTokens { get; set; } = 800;
    }
}
