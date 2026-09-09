using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;
using RagEngine.Application.DTO;
using RagEngine.Application.Interfaces;
using RagEngine.Infrastructure.Config;
using System.Diagnostics;

namespace RagEngine.Infrastructure.Cosmos
{
    public class CosmosRetriever : IRetriever
    {
        private readonly ILogger<CosmosRetriever> _logger;
        private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
        private readonly VectorStoreCollection<object, Dictionary<string, object?>> _collection;

        public CosmosRetriever(
            ILogger<CosmosRetriever> logger,
            IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
            VectorStore vectorStore,
            IOptions<CosmosDbConfig> options,
            IOptions<OllamaOptions> ollamaOptions)
        {
            _logger = logger;
            _embeddingGenerator = embeddingGenerator;

            _collection = vectorStore.GetDynamicCollection(
                options.Value.ContainerName,
                CreateCollectionDefinition(
                    ollamaOptions.Value.EmbeddingDimensions));
        }

        public async Task<IEnumerable<DocumentRetrievalResult>> SearchAsync(
            string query,
            int topK,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentException(
                    "Query cannot be null or empty.",
                    nameof(query));
            }

            if (topK <= 0 || topK > 50)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(topK),
                    topK,
                    "topK must be between 1 and 50.");
            }

            // -------- Generate query embedding --------

            var embeddingStopwatch = Stopwatch.StartNew();

            var queryEmbedding =
                await _embeddingGenerator.GenerateAsync(
                    query,
                    cancellationToken: cancellationToken);

            embeddingStopwatch.Stop();

            _logger.LogInformation(
                "Query embedding completed in {ElapsedMilliseconds:0.00} ms.",
                embeddingStopwatch.Elapsed.TotalMilliseconds);


            // -------- Vector search --------

            var searchStopwatch = Stopwatch.StartNew();

            var results =
                new List<DocumentRetrievalResult>();

            await foreach (var result in _collection.SearchAsync(
                queryEmbedding.Vector,
                top: topK,
                cancellationToken: cancellationToken))
            {
                var record = result.Record;

                var documentId =
                    record["documentid"]?.ToString()
                    ?? string.Empty;

                var content =
                    record["content"]?.ToString()
                    ?? string.Empty;

                results.Add(
                    new DocumentRetrievalResult(
                        DocumentId: documentId,
                        Content: content,
                        SimilarityScore: result.Score));
            }

            searchStopwatch.Stop();

            _logger.LogInformation(
                "Cosmos vector search completed in {ElapsedMilliseconds:0.00} ms.",
                searchStopwatch.Elapsed.TotalMilliseconds);

            return results;
        }


        // -------- Vector Store Schema --------

        private static VectorStoreCollectionDefinition CreateCollectionDefinition(
            int embeddingDimensions)
        {
            return new VectorStoreCollectionDefinition
            {
                Properties =
                {
                    new VectorStoreKeyProperty(
                        "key",
                        typeof(string)),

                    new VectorStoreVectorProperty(
                        "embedding",
                        typeof(ReadOnlyMemory<float>),
                        dimensions: embeddingDimensions)
                    {
                        DistanceFunction =
                            DistanceFunction.CosineSimilarity
                    },

                    new VectorStoreDataProperty(
                        "content",
                        typeof(string)),

                    new VectorStoreDataProperty(
                        "context",
                        typeof(string)),

                    new VectorStoreDataProperty(
                        "documentid",
                        typeof(string))
                    {
                        IsIndexed = true
                    }
                }
            };
        }
    }
}