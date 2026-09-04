using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RagEngine.Infrastructure.Config;
using System.Net.Http.Json;
using static RagEngine.Infrastructure.Embedding.GeminiEmbedRequest;

namespace RagEngine.Infrastructure.Embedding
{
    public class GeminiEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
    {
        private readonly ILogger<GeminiEmbeddingGenerator> _logger;
        private readonly GeminiOptions _options;
        private readonly HttpClient _httpClient;

        public GeminiEmbeddingGenerator(
            ILogger<GeminiEmbeddingGenerator> logger,
            IOptions<GeminiOptions> options,
            HttpClient httpClient)
        {
            _logger = logger;
            _options = options.Value;
            _httpClient = httpClient;
        }


        public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(IEnumerable<string> values, EmbeddingGenerationOptions? options = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(values);

            var inputs = values.ToArray();

            if (inputs.Length == 0) 
            {
                throw new ArgumentOutOfRangeException(nameof(inputs));
            }

            var batchRequest = new GeminiBatchEmbedRequest
            {
                Requests = inputs.Select(input => new GeminiEmbedRequest
                {
                    Model = $"models/{_options.EmbeddingModel}",
                    Content = new GeminiContent
                    {
                        Parts = 
                        [
                            new GeminiPart
                            {
                                Text = input
                            }
                        ]
                    },
                    EmbedContentConfig = new GeminiEmbedContentConfig
                    {
                        OutputDimensionality = _options.EmbeddingDimenstions
                    }
                }).ToArray()
            };

            var endpoint =$"v1beta/models/{_options.EmbeddingModel}:batchEmbedContents";

            var response = await _httpClient.PostAsJsonAsync(
                endpoint,
                batchRequest,
                cancellationToken
            );

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to generate embeddings. Status Code: {StatusCode}, Response: {Response}", response.StatusCode, errorContent);
                throw new HttpRequestException($"Failed to generate embeddings. Status Code: {response.StatusCode}, Response: {errorContent}");
            }

            var responseData = await response.Content.ReadFromJsonAsync<GeminiBatchEmbedResponse>(cancellationToken: cancellationToken);

            if (responseData?.Embeddings is null) {
                _logger.LogError("Failed to generate embeddings. Response: {Response}", responseData);
                throw new InvalidOperationException("Failed to generate embeddings.");
            }

            if (responseData.Embeddings.Length != inputs.Length)
            {
                throw new InvalidOperationException(
                    $"Gemini returned {responseData.Embeddings.Length} embeddings " +
                    $"for {inputs.Length} inputs.");
            }

            var embeddings = responseData.Embeddings.Select(embedding => new Embedding<float>(embedding.Values)).ToArray();
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
