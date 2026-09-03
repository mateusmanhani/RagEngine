using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Logging;
using RagEngine.Application.DTO;
using RagEngine.Application.Interfaces;
using RagEngine.Domain.Entities;

namespace RagEngine.Infrastructure
{
    public class AzureSearchRetriever : IRetriever
    {
        private readonly SearchClient _searchClient;
        private readonly ILogger<AzureSearchRetriever> _logger;

        public AzureSearchRetriever(SearchClient searchClient, ILogger<AzureSearchRetriever> logger)
        {
            _searchClient = searchClient;
            _logger = logger;
        }

        public async Task<IEnumerable<RetrievalResult>> SearchAsync(string query, int topK, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(query))
            {
                _logger.LogError("Search query is null or empty.");
                throw new ArgumentException("Search query cannot be null or empty.", nameof(query));
            }

            if (topK <= 0 || topK > 50)
            {
                _logger.LogError("Invalid topK value: {TopK}. It must be between 1 and 50.", topK);
                throw new ArgumentOutOfRangeException(nameof(topK), "topK must be between 1 and 50.");
            }

            var vectorQuery = new VectorizableTextQuery(query)
            {
                KNearestNeighborsCount = topK
            };

            vectorQuery.Fields.Add("snippet_vector");

            var options = new SearchOptions
            {
                Size = topK,
                VectorSearch = new VectorSearchOptions()
            };

            options.VectorSearch.Queries.Add(vectorQuery);

            options.Select.Add("uid");
            options.Select.Add("snippet_parent_id");
            options.Select.Add("blob_url");
            options.Select.Add("snippet");

            var results = new List<RetrievalResult>();

            var response = await _searchClient.SearchAsync<SearchDocument>(
                searchText: query,
                options: options,
                cancellationToken: cancellationToken);

            await foreach (var result in response.Value
                .GetResultsAsync()
                .WithCancellation(cancellationToken))
            {
                var document = result.Document;

                var id = document["uid"]?.ToString() ?? string.Empty;

                var parentId = document["snippet_parent_id"]?.ToString() ?? string.Empty;

                var content = document["snippet"]?.ToString() ?? string.Empty;

                results.Add(
                    new RetrievalResult(
                    new Chunk(
                        Id: id,
                        DocumentId: parentId,
                        ChunkIndex: 0,
                        Content: content),
                    result.Score ?? 0
                    ));
            }

            return results;

        }
    }
}
