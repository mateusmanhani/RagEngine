using RagEngine.Application.DTO;

namespace RagEngine.Application.Interfaces
{
    public interface IRetriever
    {
        Task<IEnumerable<RetrievalResult>> SearchAsync(string query, int topK, CancellationToken cancellationToken = default);
    }
}
