using RagEngine.Application.Web;

namespace RagEngine.Application.Interfaces
{
    public interface IFirecrawlClient
    {
        Task<IReadOnlyCollection<CrawledPage>> CrawlAsync (string url, CancellationToken cancellationToken = default);
    }
}
