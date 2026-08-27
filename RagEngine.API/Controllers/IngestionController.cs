using Microsoft.AspNetCore.Mvc;
using RagEngine.Application;

namespace RagEngine.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IngestionController : ControllerBase
    {
        private readonly ILogger<IngestionController> _logger;
        private readonly IngestionPipeline _ingestionPipeline;

        public IngestionController(ILogger<IngestionController> logger, IngestionPipeline ingestionPipeline)
        {
            _logger = logger;
            _ingestionPipeline = ingestionPipeline;
        }

        [HttpPost("folder")]
        public async Task<IActionResult> IngestFolder([FromQuery] string folderPath, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                return BadRequest("folderPath query parameter is required.");
            }

            try
            {
                var result = await _ingestionPipeline.IngestFolderAsync(folderPath, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while ingesting folder {FolderPath}.", folderPath);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}
