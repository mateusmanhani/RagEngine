using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RagEngine.Application.DTO;
using RagEngine.Application.Interfaces;
using RagEngine.Domain.Entities;

namespace RagEngine.Infrastructure.VectorStore
{
    public class CosmosDBVectorStore : IVectorStore
    {
        private readonly Container _container;
        private readonly ILogger<CosmosDBVectorStore> _logger;

        public CosmosDBVectorStore(IOptions<CosmosDbConfig> config, ILogger<CosmosDBVectorStore> logger)
        {
            _logger = logger;
            var options = config.Value;
            var clientOptions = new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                }
            };
            var client = new CosmosClient(options.EndpointUri, new DefaultAzureCredential(), clientOptions);
            _container = client.GetContainer(options.DatabaseName, options.OllamaContainerName);
        }

        public async Task AddAsync(IEnumerable<Chunk> chunks, CancellationToken cancellationToken = default)
        {
            foreach (var chunk in chunks) 
            {
                await _container.UpsertItemAsync(chunk, new PartitionKey(chunk.DocumentId), cancellationToken: cancellationToken);
            }
        }

        public async Task<IEnumerable<RetrievalResult>> SearchAsync(float[] embedding, int topK, CancellationToken cancellationToken = default)
        {
            if (topK <= 0 || topK > 50)
            {
                throw new ArgumentOutOfRangeException(nameof(topK), topK, "topK must be between 1 and 50.");
            }

            var queryText = $"""
                SELECT TOP {topK} c.id, c.documentId, c.chunkIndex, c.content, c.embedding,
                       VectorDistance(c.embedding, @queryEmbedding) AS distance
                FROM c
                ORDER BY VectorDistance(c.embedding, @queryEmbedding)
                """;

            var queryDefinition = new QueryDefinition(queryText)
                .WithParameter("@queryEmbedding", embedding);

            var results = new List<RetrievalResult>();

            using var iterator = _container.GetItemQueryIterator<CosmosSearchProjection>(queryDefinition);
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
                results.AddRange(response.Select(item => new RetrievalResult(
                    new Chunk(item.Id, item.DocumentId, item.ChunkIndex, item.Content, item.Embedding),
                    CosineDistanceToSimilarity(item.Distance))));
            }

            return results;
        }

        /// <summary>
        /// Shape used only to bind the projected VectorDistance score alongside chunk fields;
        /// Cosmos won't bind an ad-hoc SELECT column onto the Chunk record since it has no Score property.
        /// </summary>
        private sealed class CosmosSearchProjection
        {
            public string Id { get; set; } = string.Empty;
            public string DocumentId { get; set; } = string.Empty;
            public int ChunkIndex { get; set; }
            public string Content { get; set; } = string.Empty;
            public float[]? Embedding { get; set; }
            public double Distance { get; set; }
        }

        /// <summary>
        /// Cosmos's VectorDistance (with distanceFunction: cosine) returns cosine distance in [0, 2],
        /// where 0 = identical and 2 = opposite. We invert it to cosine similarity in [-1, 1]
        /// so Score means the same thing (higher = more relevant) across every IVectorStore implementation.
        /// </summary>
        private static double CosineDistanceToSimilarity(double cosineDistance) => 1.0 - cosineDistance;

        public async Task<IEnumerable<Chunk>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var queryText = "SELECT c.id, c.documentId, c.chunkIndex, c.content, c.embedding FROM c";

            var results = new List<Chunk>();

            using var iterator = _container.GetItemQueryIterator<Chunk>(new QueryDefinition(queryText));
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
                results.AddRange(response);
            }

            return results;
        }
    }
}
