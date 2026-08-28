using Microsoft.AspNetCore.Mvc;
using RagEngine.Application.Services;

namespace RagEngine.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RetrievalController : ControllerBase
    {
        private readonly ILogger<RetrievalController> _logger;
        private readonly RetrievalPipeline _retrievalPipeline;

        public RetrievalController(ILogger<RetrievalController> logger, RetrievalPipeline retrievalPipeline)
        {
            _logger = logger;
            _retrievalPipeline = retrievalPipeline;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string query, [FromQuery] int topK = 5, CancellationToken cancellationToken = default)
        {
            try
            {
                var results = await _retrievalPipeline.GetSimilarChunksAsync(query, topK, cancellationToken);
                return Ok(results);
            }
            catch (ArgumentException ex)
            {
                // Covers both ArgumentException (bad query) and ArgumentOutOfRangeException (bad topK).
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while searching for query {Query}.", query);
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred while searching.");
            }
        }
    }
}
