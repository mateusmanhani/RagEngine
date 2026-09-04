using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.AI;
using RagEngine.Infrastructure.Config;
using System.Net.Http.Json;

namespace RagEngine.Infrastructure.Embedding
{
    public class OllamaEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
    {
        private readonly ILogger<OllamaEmbeddingGenerator> _logger;
        private readonly HttpClient _httpClient;
        private readonly OllamaOptions _options;

        public OllamaEmbeddingGenerator(ILogger<OllamaEmbeddingGenerator> logger, HttpClient httpClient, IOptions<OllamaOptions> ollamaOptions)
        {
            _logger = logger;
            _httpClient = httpClient;
            _options = ollamaOptions.Value;
        }

        public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
            IEnumerable<string> values,
            EmbeddingGenerationOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(values);

            var inputs = values.ToArray();

            if (inputs.Length == 0)
            {
                throw new ArgumentException("At least one input is required.", nameof(values));
            }

            var request = new OllamaEmbedRequest
            {
                Model = _options.EmbeddingModel,
                Input = inputs
            };

            using var response = await _httpClient.PostAsJsonAsync(
                _options.EmbeddingEndpoint,
                request,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to generate embeddings. Status Code: {StatusCode}, Response: {Response}", response.StatusCode, errorContent);
                throw new HttpRequestException($"Failed to generate embeddings. Status Code: {response.StatusCode}, Response: {errorContent}");
            }

            var responseData = await response.Content.ReadFromJsonAsync<OllamaEmbedResponse>(cancellationToken: cancellationToken);

            if (responseData?.Embeddings is null)
            {
                throw new HttpRequestException("Failed to parse embeddings from the response.");
            }

            if (responseData.Embeddings.Length != inputs.Length)
            {
                throw new HttpRequestException($"Ollama returned {responseData.Embeddings.Length} embeddings for {inputs.Length} inputs.");
            }

            var embeddings = responseData.Embeddings
                .Select(vector => new Embedding<float>(vector))
                .ToList();

            return new GeneratedEmbeddings<Embedding<float>>(embeddings);
        }

        public object? GetService(Type serviceType, object? serviceKey = null)
        {
            if (serviceType is null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (serviceKey is not null)
            {
                return null;
            }

            if (serviceType.IsInstanceOfType(this))
            {
                return this;
            }

            return null;
        }

        public void Dispose()
        {
            // HttpClient is managed by IHttpClientFactory.
        }
    }
}
