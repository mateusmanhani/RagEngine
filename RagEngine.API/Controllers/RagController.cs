using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RagEngine.Application.Exceptions;
using RagEngine.Application.Services;

namespace RagEngine.API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class RagController : ControllerBase
    {
        private readonly ILogger<RagController> _logger;
        private readonly RagPipeline _ragPipeline;

        public RagController(ILogger<RagController> logger,  RagPipeline ragPipeline)
        {
            _logger = logger;
            _ragPipeline = ragPipeline;
        }

        [HttpGet]
        public async Task<IActionResult> AskAsync([FromQuery] string query, CancellationToken cancellationToken = default)
        {
            try
            {
                var answer = await _ragPipeline.AnswerAsync(query, cancellationToken);
                return Ok(answer);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (AnswerGenerationException ex)
            {
                _logger.LogError(ex, "Answer generation failed for query {Query}.", query);
                return StatusCode(StatusCodes.Status502BadGateway, "The answer generation service is currently unavailable.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while answering {Query}.", query);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while answering.");
            }

        }
    }
}

