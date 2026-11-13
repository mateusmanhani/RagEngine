namespace RagEngine.Application.Web
{
    public sealed record WebContentPreviewResponse(
    string RequestedUrl,
    int PageCount,
    IReadOnlyList<WebContentPreviewPage> Pages);
}
