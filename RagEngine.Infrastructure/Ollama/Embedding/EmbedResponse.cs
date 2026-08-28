namespace RagEngine.Infrastructure.Ollama.Embedding
{
    public class EmbedResponse
    {
        public string Model { get; set; } = string.Empty;
        public float[][] Embeddings { get; set; } = [];
    }
}
