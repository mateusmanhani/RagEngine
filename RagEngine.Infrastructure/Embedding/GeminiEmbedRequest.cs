using System.Text.Json.Serialization;

namespace RagEngine.Infrastructure.Embedding
{
    public class GeminiEmbedRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        public GeminiContent Content { get; set; } = new();

        [JsonPropertyName("embedContentConfig")]
        public GeminiEmbedContentConfig EmbedContentConfig { get; set; } = new();

        public class GeminiEmbedContentConfig
        {
            [JsonPropertyName("outputDimensionality")]
            public int OutputDimensionality { get; set; }
        }

        public class GeminiContent
        {
            [JsonPropertyName("parts")]
            public GeminiPart[] Parts { get; set; } = [];
        }

        public class GeminiPart
        {
            [JsonPropertyName("text")]
            public string Text { get; set; } = string.Empty;
        }
    }

    public class GeminiBatchEmbedRequest
    {
        [JsonPropertyName("requests")]
        public GeminiEmbedRequest[] Requests { get; set; } = [];
    }
}