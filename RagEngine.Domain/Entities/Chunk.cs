namespace RagEngine.Domain.Entities
{
    public record Chunk(
        string Id,
        string DocumentId,
        int ChunkIndex,
        string Content,
        float[]? Embedding = null);
}
