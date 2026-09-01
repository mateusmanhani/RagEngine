using System.Diagnostics;
using Microsoft.Extensions.Logging;
using RagEngine.Application.DTO;
using RagEngine.Application.Interfaces;

namespace RagEngine.Application.Services
{
    public class CosmosRetriever : IRetriever
    {
        private readonly ILogger<CosmosRetriever> _logger;
        private readonly IEmbeddingGenerator _embeddingGenerator;
        private readonly IVectorStore _vectorStore;

        public CosmosRetriever(
            ILogger<CosmosRetriever> logger,
            IEmbeddingGenerator embeddingGenerator,
            IVectorStore vectorStore)
        {
            _logger = logger;
            _embeddingGenerator = embeddingGenerator;
            _vectorStore = vectorStore;
        }

        public async Task<IEnumerable<RetrievalResult>> SearchAsync(string query, int topK, CancellationToken cancellationToken = default)
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

            var embeddingStopwatch = Stopwatch.StartNew();
            var queryEmbedding = await _embeddingGenerator.GenerateEmbeddingAsync(query, cancellationToken);
            embeddingStopwatch.Stop();
            _logger.LogInformation(
                "Query embedding completed in {ElapsedMilliseconds:0.00} ms for query {Query}.",
                embeddingStopwatch.Elapsed.TotalMilliseconds,
                query);

            var searchStopwatch = Stopwatch.StartNew();
            var similarChunks = await _vectorStore.SearchAsync(queryEmbedding, topK, cancellationToken);
            searchStopwatch.Stop();
            _logger.LogInformation(
                "Cosmos vector search completed in {ElapsedMilliseconds:0.00} ms for query {Query}.",
                searchStopwatch.Elapsed.TotalMilliseconds,
                query);

            return similarChunks;
        }
    }
}
