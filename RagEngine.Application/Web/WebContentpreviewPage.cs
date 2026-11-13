namespace RagEngine.Application.Web

{
    public sealed record WebContentPreviewPage(
    string Url,
    string? Title,
    string? Description,
    string OriginalMarkdown,
    string CleanedMarkdown);
}
