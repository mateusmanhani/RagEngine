namespace RagEngine.Infrastructure.Embedding
{
    public class OllamaEmbedResponse
    {
        public string Model { get; set; } = string.Empty;
        public float[][] Embeddings { get; set; } = [];
    }
}
