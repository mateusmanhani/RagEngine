using RagEngine.Domain.Entities;

namespace RagEngine.Application.Interfaces
{
    public interface IVectorStore
    {
        Task AddAsync(IEnumerable<Chunk> chunks, CancellationToken cancellationToken = default);
        Task<IEnumerable<Chunk>> SearchAsync(float[] embedding, int topK, CancellationToken cancellationToken = default);
    }
}
