using RagEngine.Application.DTO;

namespace RagEngine.Application.Interfaces
{
    public interface IRetriever
    {
        Task<IEnumerable<DocumentRetrievalResult>> SearchAsync(string query, int topK, CancellationToken cancellationToken = default);
    }
}
