namespace RagEngine.Application.Web
{
    public sealed record CrawledPage(
    string Url,
    string? Title,
    string Markdown,
    string? Description);
}
