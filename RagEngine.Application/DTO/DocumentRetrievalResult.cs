namespace RagEngine.Application.DTO
{
    public record DocumentRetrievalResult(string DocumentId,string Content, double? SimilarityScore);
}
