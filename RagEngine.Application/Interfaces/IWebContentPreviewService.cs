using RagEngine.Application.Web;

namespace RagEngine.Application.Interfaces
{
    public interface IWebContentPreviewService
    {
        Task<WebContentPreviewResponse> PreviewAsync(
            string url,
            CancellationToken cancellationToken = default);
    }

}
