using Microsoft.AspNetCore.Mvc;
using RagEngine.Application.Interfaces;

namespace RagEngine.API.Controllers
{
    /// <summary>
    /// Small inspection endpoint to look at what is currently sitting in the vector store,
    /// without needing to query Cosmos DB or add real retrieval logic yet.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DiagnosticsController : ControllerBase
    {
        private readonly IVectorStore _vectorStore;

        public DiagnosticsController(IVectorStore vectorStore)
        {
            _vectorStore = vectorStore;
        }

        [HttpGet("chunks")]
        public async Task<IActionResult> GetChunks([FromQuery] string? documentId, CancellationToken cancellationToken)
        {
            var chunks = await _vectorStore.GetAllAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(documentId))
            {
                chunks = chunks.Where(c => c.DocumentId == documentId);
            }

            var summary = chunks
                .Select(c => new
                {
                    c.Id,
                    c.DocumentId,
                    c.ChunkIndex,
                    ContentPreview = BuildPreview(c.Content, 200),
                    ContentLength = c.Content.Length,
                    HasEmbedding = c.Embedding is not null,
                    EmbeddingDimensions = c.Embedding?.Length ?? 0
                })
                .OrderBy(c => c.DocumentId)
                .ThenBy(c => c.ChunkIndex);

            return Ok(new
            {
                TotalChunks = summary.Count(),
                Chunks = summary
            });
        }

        /// <summary>
        /// Truncates content for display, backing off to the nearest preceding whitespace
        /// so previews don't cut a word in half.
        /// </summary>
        private static string BuildPreview(string content, int maxLength)
        {
            if (content.Length <= maxLength)
            {
                return content;
            }

            var cutoff = content.LastIndexOf(' ', maxLength - 1);
            if (cutoff <= 0)
            {
                cutoff = maxLength;
            }

            return content[..cutoff].TrimEnd() + "...";
        }
    }
}
