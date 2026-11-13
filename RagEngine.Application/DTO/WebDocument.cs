namespace RagEngine.Application.DTO
{
    public record WebDocument(
        string Url,
        string Title,
        string Content,
        DateTime RetrivedAt,
        DateTime? PublishedAt = null );
}
