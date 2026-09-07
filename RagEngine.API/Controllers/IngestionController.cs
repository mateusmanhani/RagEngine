using Microsoft.AspNetCore.Mvc;
using RagEngine.Application.Services;

namespace RagEngine.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IngestionController : ControllerBase
    {
        private readonly ILogger<IngestionController> _logger;
        private readonly DocumentIngestionService _ingestionService;

        public IngestionController(ILogger<IngestionController> logger, DocumentIngestionService ingestionService)
        {
            _logger = logger;
            _ingestionService = ingestionService;
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
                var result = await _ingestionService.IngestFolderAsync(folderPath, cancellationToken);
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
