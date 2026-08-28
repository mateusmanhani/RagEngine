using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RagEngine.Application.DTO;
using RagEngine.Application.Exceptions;
using RagEngine.Application.Interfaces;

namespace RagEngine.Infrastructure.Ollama.Synthesis
{
    public class OllamaAnswerGenerator : IAnswerGenerator
    {
        private readonly ILogger<OllamaAnswerGenerator> _logger;
        private readonly IOptions<OllamaOptions> _options;
        private readonly HttpClient _httpClient;

        public OllamaAnswerGenerator(ILogger<OllamaAnswerGenerator> logger, IOptions<OllamaOptions> options, HttpClient httpClient)
        {
            _logger = logger;
            _options = options;
            _httpClient = httpClient;
        }

        public async Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                _logger.LogError("Prompt cannot be null or whitespace.");
                throw new ArgumentException("Prompt cannot be null or whitespace.", nameof(prompt));
            }

            var options = _options.Value;

            var generateRequest = new GenerateRequest
            {
                Model = options.GenerationModel,
                Prompt = prompt
            };

            using var response = await _httpClient.PostAsJsonAsync(options.GenerationEndpoint, generateRequest, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to generate answer. Status code: {StatusCode}, Reason: {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
                throw new AnswerGenerationException($"Failed to generate answer. Status code: {response.StatusCode}, Reason: {response.ReasonPhrase}");
            }

            var responseContent = await response.Content.ReadFromJsonAsync<GenerateResponse>(cancellationToken: cancellationToken);

            if (responseContent == null)
            {
                _logger.LogError("Failed to read response content.");
                throw new AnswerGenerationException("Failed to read response content.");
            }
            if (string.IsNullOrWhiteSpace(responseContent.Response))
            {
                _logger.LogError("Response content is empty.");
                throw new AnswerGenerationException("Ollama returned an empty response.");
            }

            return responseContent.Response;
        }
    }
}
