using Microsoft.Extensions.Options;
using RagEngine.Application.Interfaces;
using RagEngine.Application.Web;
using RagEngine.Infrastructure.Config;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace RagEngine.Infrastructure.Web
{
    public class FirecrawlClient : IFirecrawlClient
    {
        private readonly HttpClient _httpClient;
        private readonly FirecrawlOptions _options;

        public FirecrawlClient(HttpClient httpClient, IOptions<FirecrawlOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<IReadOnlyCollection<CrawledPage>> CrawlAsync(string url, CancellationToken cancellationToken = default)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
            {
                throw new ArgumentException("Invalid URL", nameof(url));
            }

            var request = new
            {
                url,
                limit = _options.CrawlLimit,
                scrapeOptions = new
                {
                    formats = new[] { "markdown" }
                }
            };

            using var response = await _httpClient.PostAsJsonAsync(
                "v2/crawl",
                request,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var job = await response.Content.ReadFromJsonAsync<CrawlStartResponse>(cancellationToken: cancellationToken);
            
            if (string.IsNullOrWhiteSpace(job?.Id))
            {
                throw new InvalidOperationException("Firecrawl did not return a crawl job ID.");
            }

            var status = await WaitForCompletionAsync(job.Id, cancellationToken);

            var pages = new List<CrawledPage>();

            await CollectPagesAsync(status, pages, cancellationToken);

            return pages;
        }

        private async Task<CrawlStatusResponse> WaitForCompletionAsync(
        string jobId,
        CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();

            while (stopwatch.Elapsed < TimeSpan.FromMinutes(_options.MaxWaitMinutes))
            {
                cancellationToken.ThrowIfCancellationRequested();

                using var response = await _httpClient.GetAsync(
                    $"v2/crawl/{Uri.EscapeDataString(jobId)}",
                    cancellationToken);

                response.EnsureSuccessStatusCode();

                var status = await response.Content.ReadFromJsonAsync<CrawlStatusResponse>(
                    cancellationToken: cancellationToken)
                    ?? throw new InvalidOperationException("Invalid Firecrawl response.");

                if (status.Status == "completed")
                    return status;

                if (status.Status == "failed")
                    throw new InvalidOperationException(
                        $"Firecrawl crawl failed: {status.Error}");

                await Task.Delay(
                    TimeSpan.FromSeconds(_options.PollIntervalSeconds),
                    cancellationToken);
            }

            throw new TimeoutException(
                $"Firecrawl crawl did not complete within {_options.MaxWaitMinutes} minutes.");
        }


        private async Task CollectPagesAsync(
        CrawlStatusResponse status,
        List<CrawledPage> pages,
        CancellationToken cancellationToken)
        {
            AddPages(status, pages);

            var next = status.Next;

            while (!string.IsNullOrWhiteSpace(next))
            {
                var nextUri = new Uri(_httpClient.BaseAddress!, next);

                // Only follow pagination links from the Firecrawl API host.
                if (nextUri.Host != _httpClient.BaseAddress!.Host)
                    throw new InvalidOperationException("Unexpected Firecrawl pagination URL.");

                using var response = await _httpClient.GetAsync(
                    nextUri,
                    cancellationToken);

                response.EnsureSuccessStatusCode();

                var nextPage = await response.Content.ReadFromJsonAsync<CrawlStatusResponse>(
                    cancellationToken: cancellationToken)
                    ?? throw new InvalidOperationException("Invalid Firecrawl pagination response.");

                AddPages(nextPage, pages);
                next = nextPage.Next;
            }
        }


        private static void AddPages(
        CrawlStatusResponse response,
        List<CrawledPage> pages)
        {
            if (response.Data is null)
                return;

            foreach (var page in response.Data)
            {
                if (string.IsNullOrWhiteSpace(page.Markdown) ||
                    string.IsNullOrWhiteSpace(page.Metadata?.SourceUrl))
                    continue;

                pages.Add(new CrawledPage(
                    page.Metadata.SourceUrl,
                    page.Metadata.Title,
                    page.Markdown,
                    page.Metadata.Description));
            }
        }

        private sealed class CrawlStartResponse
        {
            public string? Id { get; set; }
        }

        private sealed class CrawlStatusResponse
        {
            public string? Status { get; set; }
            public string? Error { get; set; }
            public string? Next { get; set; }
            public List<CrawlPageResponse>? Data { get; set; }
        }

        private sealed class CrawlPageResponse
        {
            public string? Markdown { get; set; }
            public CrawlMetadata? Metadata { get; set; }
        }

        private sealed class CrawlMetadata
        {
            [JsonPropertyName("sourceURL")]
            public string? SourceUrl { get; set; }

            public string? Title { get; set; }

            public string? Description { get; set; }
        }
    }
}
