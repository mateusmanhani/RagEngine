namespace RagEngine.Application.DTO
{
    public record DocumentRetrievalResult(
        string DocumentId,
        string Content,
        double? SimilarityScore,
        string? ChunkId = null,
        string? ParentDocumentId = null,
        string? SourceUrl = null);
}
