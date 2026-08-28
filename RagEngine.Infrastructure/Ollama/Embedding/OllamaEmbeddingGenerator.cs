using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RagEngine.Application.Interfaces;
using System.Net.Http.Json;

namespace RagEngine.Infrastructure.Ollama.Embedding
{
    public class OllamaEmbeddingGenerator : IEmbeddingGenerator
    {
        private readonly ILogger<OllamaEmbeddingGenerator> _logger;
        private readonly HttpClient _httpClient;
        private readonly IOptions<OllamaOptions> _ollamaOptions;

        public OllamaEmbeddingGenerator(ILogger<OllamaEmbeddingGenerator> logger, HttpClient httpClient, IOptions<OllamaOptions> ollamaOptions)
        {
            _logger = logger;
            _httpClient = httpClient;
            _ollamaOptions = ollamaOptions;
        }

        public async Task<float[]> GenerateEmbeddingAsync(string input, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(input);

            var embeddings = await GenerateEmbeddingsAsync([input], cancellationToken);
            return embeddings[0];
        }

        public async Task<IReadOnlyList<float[]>> GenerateEmbeddingsAsync(IReadOnlyList<string> inputs, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(inputs);

            if (inputs.Count == 0)
            {
                throw new ArgumentException("At least one input is required.", nameof(inputs));
            }

            var options = _ollamaOptions.Value;

            var request = new EmbedRequest
            {
                Model = options.EmbeddingModel,
                Input = inputs.ToArray()
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

            if (responseData.Embeddings is null || responseData.Embeddings.Length != inputs.Count)
            {
                throw new HttpRequestException(
                    $"Ollama returned {responseData.Embeddings?.Length ?? 0} embeddings, expected {inputs.Count}."
                );
            }

            foreach (var embedding in responseData.Embeddings)
            {
                if (embedding is null || embedding.Length != options.EmbeddingDimensions)
                {
                    throw new HttpRequestException($"Ollama returned an embedding of unexpected length: {embedding?.Length ?? 0}. Expected length is {options.EmbeddingDimensions}.");
                }
            }

            return responseData.Embeddings;
        }

    }
}
