
using Microsoft.AspNetCore.Mvc;
using RagEngine.Application.Interfaces;
using RagEngine.Application.Web;

namespace RagEngine.API.Controllers;

[ApiController]
[Route("api/ingestion/web")]
public sealed class WebIngestionController : ControllerBase
{
    private readonly IWebContentPreviewService _previewService;

    public WebIngestionController(
        IWebContentPreviewService previewService)
    {
        _previewService = previewService;
    }

    [HttpPost("preview")]
    [ProducesResponseType(typeof(WebContentPreviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WebContentPreviewResponse>> Preview(
        [FromBody] WebContentPreviewRequest request,
        CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp &&
             uri.Scheme != Uri.UriSchemeHttps))
        {
            ModelState.AddModelError(
                nameof(request.Url),
                "A valid HTTP or HTTPS URL is required.");

            return ValidationProblem(ModelState);
        }

        var result = await _previewService.PreviewAsync(
            uri.ToString(),
            cancellationToken);

        return Ok(result);
    }
}
