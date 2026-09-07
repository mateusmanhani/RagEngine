using Microsoft.Extensions.DataIngestion;
using Microsoft.Extensions.Logging;
using RagEngine.Application.DTO;

namespace RagEngine.Application.Services
{
    public class DocumentIngestionService
    {
        private readonly ILogger<DocumentIngestionService> _logger;
        private readonly IngestionPipeline<string> _pipeline;

        public DocumentIngestionService(ILogger<DocumentIngestionService> logger, IngestionPipeline<string> pipeline)
        {
            _logger = logger;
            _pipeline = pipeline;
        }

        public async Task<FolderIngestionResult> IngestFolderAsync(string folderPath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                throw new ArgumentException("Folder path cannot be null or empty.", nameof(folderPath));
            }

            var directory = new DirectoryInfo(folderPath);

            if (!directory.Exists) {
                throw new DirectoryNotFoundException("Folder not found.");
            }

            var succeeded = 0;
            var failed = 0;

            await foreach( var result in _pipeline.ProcessAsync(
                directory,
                searchPattern: "*.pdf",
                cancellationToken: cancellationToken))
            {
                if (result.Succeeded)
                {
                    succeeded++;

                    _logger.LogInformation(
                        "Successfully ingested {DocumentId}.",
                        result.DocumentId);
                }
                else
                {
                    failed++;

                    _logger.LogError(
                        result.Exception,
                        "Failed to ingest {DocumentId}.",
                        result.DocumentId);
                }
            }

            return new FolderIngestionResult(succeeded, failed);
        }
    }
}
