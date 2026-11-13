namespace RagEngine.Application.DTO
{
    public sealed record CrawledPage(
    string Url,
    string? Title,
    string Markdown,
    string? Description);
}
