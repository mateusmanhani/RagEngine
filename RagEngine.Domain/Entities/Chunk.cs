namespace RagEngine.Domain.Entities
{
    public record Chunk(
        string Id,
        string DocumentId,
        int ChunkIndex,
        string content,
        float[]? Embedding = null);
}
