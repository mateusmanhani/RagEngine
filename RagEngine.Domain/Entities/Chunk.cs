using Newtonsoft.Json;

namespace RagEngine.Domain.Entities
{
    public record Chunk(
        [property: JsonProperty("id")] string Id,
        [property: JsonProperty("documentID")] string DocumentId,
        [property: JsonProperty("chunkIndex")] int ChunkIndex,
        [property: JsonProperty("content")] string Content,
        [property: JsonProperty("embedding")] float[]? Embedding = null);
}