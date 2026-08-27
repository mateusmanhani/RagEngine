using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
            _container = client.GetContainer(options.DatabaseName, options.ContainerName);
        }

        public async Task AddAsync(IEnumerable<Chunk> chunks, CancellationToken cancellationToken = default)
        {
            foreach (var chunk in chunks) 
            {
                await _container.UpsertItemAsync(chunk, new PartitionKey(chunk.DocumentId), cancellationToken: cancellationToken);
            }
        }

        public async Task<IEnumerable<Chunk>> SearchAsync(float[] embedding, int topK, CancellationToken cancellationToken = default)
        {
            if (topK <= 0 || topK > 50)
            {
                throw new ArgumentOutOfRangeException(nameof(topK), topK, "topK must be between 1 and 50.");
            }

            var queryText = $"""
                SELECT TOP {topK} c.id, c.documentId, c.chunkIndex, c.content, c.embedding
                FROM c
                ORDER BY VectorDistance(c.embedding, @queryEmbedding)
                """;

            var queryDefinition = new QueryDefinition(queryText)
                .WithParameter("@queryEmbedding", embedding);

            var results = new List<Chunk>();

            using var iterator = _container.GetItemQueryIterator<Chunk>(queryDefinition);
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync(cancellationToken);
                results.AddRange(response);
            }

            return results;
        }

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
