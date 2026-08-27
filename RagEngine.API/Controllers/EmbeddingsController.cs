using Microsoft.AspNetCore.Mvc;
using RagEngine.Application.Interfaces;

namespace RagEngine.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmbeddingsController : ControllerBase
    {
        private readonly ILogger<EmbeddingsController> _logger;
        private readonly IEmbeddingGenerator _embeddingService;

        public EmbeddingsController(ILogger<EmbeddingsController> logger, IEmbeddingGenerator embeddingService)
        {
            _logger = logger;
            _embeddingService = embeddingService;
        }

        [HttpPost]
        public async Task<IActionResult> GenerateEmbedding([FromBody] string input, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return BadRequest("Input cannot be null or whitespace.");
            }
            try
            {
                var embedding = await _embeddingService.GenerateEmbeddingAsync(input, cancellationToken);
                return Ok(embedding);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting embedding for input: {Input}", input);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}
