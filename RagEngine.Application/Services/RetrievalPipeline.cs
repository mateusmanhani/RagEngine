using Microsoft.Extensions.Logging;
using RagEngine.Application.DTO;
using RagEngine.Application.Interfaces;
using RagEngine.Domain.Entities;

namespace RagEngine.Application.Services
{
    public class RetrievalPipeline
    {
        private readonly ILogger<RetrievalPipeline> _logger;
        private readonly IEmbeddingGenerator _embeddingGenerator;
        private readonly IVectorStore _vectorStore;

        public RetrievalPipeline(
            ILogger<RetrievalPipeline> logger,
            IEmbeddingGenerator embeddingGenerator,
            IVectorStore vectorStore)
        {
            _logger = logger;
            _embeddingGenerator = embeddingGenerator;
            _vectorStore = vectorStore;
        }

        public async Task<IEnumerable<RetrievalResult>> GetSimilarChunksAsync(string query, int topK, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(query))
            {
                _logger.LogError("Query cannot be null or empty.");
                throw new ArgumentException("Query cannot be null or empty.", nameof(query));
            }

            if (topK <= 0 || topK > 50)
            {
                _logger.LogError("topK must be between 1 and 50, but was {TopK}.", topK);
                throw new ArgumentOutOfRangeException(nameof(topK), topK, "topK must be between 1 and 50.");
            }

            var queryEmbedding = await _embeddingGenerator.GenerateEmbeddingAsync(query, cancellationToken);
            var similarChunks = await _vectorStore.SearchAsync(queryEmbedding, topK, cancellationToken);
            return similarChunks;
        }
    }
}
