using System.Diagnostics;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RagEngine.Application.Exceptions;
using RagEngine.Application.Interfaces;
using RagEngine.Infrastructure.Config;

namespace RagEngine.Infrastructure.Synthesis
{
    public class OllamaAnswerGenerator : IAnswerGenerator
    {
        private readonly ILogger<OllamaAnswerGenerator> _logger;
        private readonly OllamaOptions _options;
        private readonly HttpClient _httpClient;

        public OllamaAnswerGenerator(
            ILogger<OllamaAnswerGenerator> logger,
            IOptions<OllamaOptions> options,
            HttpClient httpClient)
        {
            _logger = logger;
            _options = options.Value;
            _httpClient = httpClient;
        }

        public async Task<string> GenerateAsync(
            string prompt,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                throw new ArgumentException(
                    "Prompt cannot be null or whitespace.",
                    nameof(prompt));
            }

            var generationStopwatch = Stopwatch.StartNew();

            try
            {
                var generateRequest = new OllamaGenerateRequest
                {
                    Model = _options.GenerationModel,
                    Prompt = prompt,
                    Stream = false,
                    KeepAlive = _options.GenerationKeepAlive,
                    Think = _options.GenerationThink,

                    Options = new GenerateOptions
                    {
                        Temperature = _options.GenerationTemperature,
                        NumPredict = _options.GenerationMaxTokens
                    }
                };

                using var response = await _httpClient.PostAsJsonAsync(
                    _options.GenerationEndpoint,
                    generateRequest,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent =
                        await response.Content.ReadAsStringAsync(cancellationToken);

                    _logger.LogError(
                        "Ollama generation failed. StatusCode: {StatusCode}, Reason: {Reason}, Response: {Response}",
                        response.StatusCode,
                        response.ReasonPhrase,
                        errorContent);

                    throw new AnswerGenerationException(
                        $"Ollama generation failed with status code {response.StatusCode}.");
                }

                var responseContent =
                    await response.Content.ReadFromJsonAsync<GenerateResponse>(
                        cancellationToken: cancellationToken);

                if (responseContent == null)
                {
                    throw new AnswerGenerationException(
                        "Failed to read Ollama response.");
                }
                
                if (string.IsNullOrWhiteSpace(responseContent.Response))
                {
                    throw new AnswerGenerationException(
                        "Ollama returned an empty response.");
                }

                LogOllamaMetrics(responseContent, prompt);

                return responseContent.Response;
            }
            finally
            {
                generationStopwatch.Stop();

                _logger.LogInformation(
                    "Ollama generation completed in {ElapsedMilliseconds:F2} ms.",
                    generationStopwatch.Elapsed.TotalMilliseconds);
            }
        }

        private void LogOllamaMetrics(
            GenerateResponse response,
            string prompt)
        {
            const double NanosecondsPerMillisecond = 1_000_000.0;

            var totalMilliseconds =
                response.TotalDuration / NanosecondsPerMillisecond;

            var loadMilliseconds =
                response.LoadDuration / NanosecondsPerMillisecond;

            var promptEvalMilliseconds =
                response.PromptEvalDuration / NanosecondsPerMillisecond;

            var evalMilliseconds =
                response.EvalDuration / NanosecondsPerMillisecond;

            var tokensPerSecond =
                response.EvalDuration > 0
                    ? response.EvalCount /
                      (response.EvalDuration / 1_000_000_000.0)
                    : 0;

            _logger.LogInformation(
                """
                Ollama metrics:
                Model: {Model}
                Prompt characters: {PromptCharacters}
                Total: {TotalMs:F2} ms
                Load: {LoadMs:F2} ms
                Prompt tokens: {PromptTokens}
                Prompt evaluation: {PromptEvalMs:F2} ms
                Generated tokens: {GeneratedTokens}
                Generation: {EvalMs:F2} ms
                Generation speed: {TokensPerSecond:F2} tokens/sec
                """,
                _options.GenerationModel,
                prompt.Length,
                totalMilliseconds,
                loadMilliseconds,
                response.PromptEvalCount,
                promptEvalMilliseconds,
                response.EvalCount,
                evalMilliseconds,
                tokensPerSecond);
        }
    }
}