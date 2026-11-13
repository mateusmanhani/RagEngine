
using RagEngine.Application.Interfaces;
using RagEngine.Application.Web;

namespace RagEngine.Application.Services;

public sealed class WebContentPreviewService : IWebContentPreviewService
{
    private readonly IFirecrawlClient _firecrawlClient;
    private readonly IWebContentCleaner _contentCleaner;

    public WebContentPreviewService(
        IFirecrawlClient firecrawlClient,
        IWebContentCleaner contentCleaner)
    {
        _firecrawlClient = firecrawlClient;
        _contentCleaner = contentCleaner;
    }

    public async Task<WebContentPreviewResponse> PreviewAsync(
        string url,
        CancellationToken cancellationToken = default)
    {
        var crawledPages = await _firecrawlClient.CrawlAsync(
            url,
            cancellationToken);

        var pages = crawledPages
            .Select(page => new WebContentPreviewPage(
                Url: page.Url,
                Title: page.Title,
                Description: page.Description,
                OriginalMarkdown: page.Markdown,
                CleanedMarkdown: _contentCleaner.Clean(page.Markdown)))
            .ToList();

        return new WebContentPreviewResponse(
            RequestedUrl: url,
            PageCount: pages.Count,
            Pages: pages);
    }
}
