using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RagEngine.Application.Interfaces;
using System.Net.Http.Json;

namespace RagEngine.Infrastructure.Embedding
{
    public class OllamaEmbeddingService : IEmbeddingService
    {
        private readonly ILogger<OllamaEmbeddingService> _logger;
        private readonly HttpClient _httpClient;
        private readonly IOptions<OllamaOptions> _ollamaOptions;

        public OllamaEmbeddingService(ILogger<OllamaEmbeddingService> logger, HttpClient httpClient, IOptions<OllamaOptions> ollamaOptions)
        {
            _logger = logger;
            _httpClient = httpClient;
            _ollamaOptions = ollamaOptions;
        }

        public async Task<float[]> GetEmbeddingAsync(string input, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(input);

            var options = _ollamaOptions.Value;

            var request = new EmbedRequest
            {
                Model = options.EmbeddingModel,
                Input = input
            };

            using var response = await _httpClient.PostAsJsonAsync(options.EmbeddingEndpoint, request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Ollama embedding request failed with status code {StatusCode}",
                    response.StatusCode
                );

                throw new HttpRequestException(
                    $"Ollama embedding request failed with status code {response.StatusCode}"
                );
            }

            var responseData = await response.Content.ReadFromJsonAsync<EmbedResponse>(cancellationToken: cancellationToken);

            if (responseData is null)
            {
                throw new HttpRequestException("Ollama returned an empty embedding response.");
            }

            if (responseData.Embeddings is null || responseData.Embeddings.Length == 0)
            {
                throw new HttpRequestException("Ollama returned an empty embeddings array.");
            }

            var embedding = responseData.Embeddings[0];

            if (embedding is null || embedding.Length != options.EmbeddingDimensions)
            {
                throw new HttpRequestException($"Ollama returned an embedding of unexpected length: {embedding?.Length ?? 0}. Expected length is {options.EmbeddingDimensions}.");
            }

            return embedding;

        }

    }
}
