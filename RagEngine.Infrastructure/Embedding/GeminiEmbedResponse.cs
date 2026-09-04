using System.Text.Json.Serialization;

namespace RagEngine.Infrastructure.Embedding
{
    public class GeminiEmbedResponse
    {
        [JsonPropertyName("embedding")]
        public GeminiEmbedding Embedding { get; set; } = new();

        public class GeminiEmbedding
        {
            [JsonPropertyName("values")]
            public float[] Values { get; set; } = [];
        }
    }

    public class GeminiBatchEmbedResponse
    {
        [JsonPropertyName("embeddings")]
        public GeminiEmbedResponse.GeminiEmbedding[] Embeddings { get; set; } = [];
    }
}
