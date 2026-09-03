using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RagEngine.Application.Exceptions;
using RagEngine.Application.Interfaces;
using RagEngine.Infrastructure.Config;
using System.Diagnostics;
using System.Net.Http.Json;

namespace RagEngine.Infrastructure.Synthesis
{
    public class GroqAnswerGenerator : IAnswerGenerator
    {
        private readonly ILogger<GroqAnswerGenerator> _logger;
        private readonly HttpClient _httpClient;
        private readonly GroqOptions _options;

        public GroqAnswerGenerator(ILogger<GroqAnswerGenerator> logger, HttpClient httpClient, IOptions<GroqOptions> options)
        {
            _logger = logger;
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                _logger.LogError("Prompt cannot be null or whitespace.");
                throw new ArgumentNullException(nameof(prompt));
            }

            var generationStopwatch = Stopwatch.StartNew();

            try
            {
                var generateRequest = new GroqChatRequest
                {
                    Model = _options.Model,
                    Messages = [
                        new GroqMessage
                        {
                            Role = "user",
                            Content = prompt
                        }
                    ],
                    Temperature = _options.Temperature,
                    MaxCompletionTokens = _options.MaxCompletionTokens
                };

                using var response = await _httpClient.PostAsJsonAsync(_options.Endpoint, generateRequest, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);

                    _logger.LogError(
                        "Groq generation failed. Status: {StatusCode}. Error: {Error}",
                        response.StatusCode,
                        errorContent);

                    throw new AnswerGenerationException(
                        $"Groq generation failed with status code {response.StatusCode}.");
                }

                var responseContent = await response.Content.ReadFromJsonAsync<GroqChatResponse>(cancellationToken);

                var answer = responseContent?
                    .Choices
                    .FirstOrDefault()?
                    .Message?
                    .Content;

                if (string.IsNullOrWhiteSpace(answer))
                {
                    throw new AnswerGenerationException(
                        "Groq returned an empty response.");
                }

                LogMetrics(responseContent, prompt, generationStopwatch.Elapsed);

                return answer;

            }
            finally
            {
                generationStopwatch.Stop();

                _logger.LogInformation(
                    "Groq generation completed in {ElapsedMilliseconds:F2} ms.",
                    generationStopwatch.Elapsed.TotalMilliseconds);
            }
        }

        private void LogMetrics(GroqChatResponse response, string prompt, TimeSpan elapsed)
        {
            var tokensPerSecond =
                elapsed.TotalSeconds > 0
                    ? response.Usage?.CompletionTokens / elapsed.TotalSeconds ?? 0
                    : 0;

            _logger.LogInformation(
                """
                Groq metrics:
                Model: {Model}
                Prompt characters: {PromptCharacters}
                Prompt tokens: {PromptTokens}
                Generated tokens: {GeneratedTokens}
                Total tokens: {TotalTokens}
                Total API time: {TotalMs:F2} ms
                Approximate generation speed: {TokensPerSecond:F2} tokens/sec
                """,
                _options.Model,
                prompt.Length,
                response.Usage?.PromptTokens ?? 0,
                response.Usage?.CompletionTokens ?? 0,
                response.Usage?.TotalTokens ?? 0,
                elapsed.TotalMilliseconds,
                tokensPerSecond);
        }
    }
}
