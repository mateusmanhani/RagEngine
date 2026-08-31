using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RagEngine.Application.Interfaces;
using RagEngine.Infrastructure.Config;

namespace RagEngine.Infrastructure.Synthesis
{
    public class GroqAnswerGenerator 
    {
        private readonly ILogger<GroqAnswerGenerator> _logger;
        private readonly HttpClient _httpClient;
        private readonly IOptions<GroqOptions> _options;
    }
}
 