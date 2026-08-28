using System.Collections.Concurrent;
using RagEngine.Application.Interfaces;
using RagEngine.Domain.Entities;
using System.Numerics.Tensors;
using RagEngine.Application.DTO;

namespace RagEngine.Infrastructure.VectorStore
{
    public class InMemoryVectorStore : IVectorStore
    {
        private readonly ConcurrentBag<Chunk> _chunks = new();

        public Task AddAsync(IEnumerable<Chunk> chunks, CancellationToken cancellationToken = default)
        {
            foreach (var chunk in chunks)
            {
                if (chunk is not null)
                    _chunks.Add(chunk);
            }

            return Task.CompletedTask;
        }

        public Task<IEnumerable<RetrievalResult>> SearchAsync(float[] embedding, int topK, CancellationToken cancellationToken = default)
        {
            var results = _chunks
                .Where(c => c.Embedding is not null)
                .Select(c => new RetrievalResult(c, TensorPrimitives.CosineSimilarity(embedding, c.Embedding!)))
                .OrderByDescending(r => r.SimilarityScore)
                .Take(topK);

            return Task.FromResult<IEnumerable<RetrievalResult>>(results);
        }

        public Task<IEnumerable<Chunk>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IEnumerable<Chunk>>(_chunks.ToList());
        }
    }
}
