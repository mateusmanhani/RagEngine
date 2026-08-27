using Microsoft.Extensions.Logging;
using RagEngine.Application.Interfaces;
using RagEngine.Domain.Entities;
using RagEngine.Domain.Enums;

namespace RagEngine.Infrastructure.DocumentIngestion
{
    public class DocumentLoader : IDocumentLoader
    {
        private readonly ILogger<DocumentLoader> _logger;


        public DocumentLoader(ILogger<DocumentLoader> logger)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<Document>> LoadFromFolderAsync(string folderPath, CancellationToken cancellationToken = default)
        {
            try
            {
                var documents = new List<Document>();
                // read array of documents from folderPath and return them
                foreach (var path in Directory.EnumerateFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly))
                {
                    var content = await File.ReadAllTextAsync(path, cancellationToken);
                    if (string.IsNullOrWhiteSpace(content))
                    {
                        _logger.LogWarning("File {FilePath} is empty or could not be read.", path);
                        continue;
                    }
                    var format = GetFormatFromExtension(path);
                    if (format == null)
                    {
                        _logger.LogWarning("Skipping unsupported file type: {FilePath}.", path); 
                        continue;
                    }

                    var document = new Document(
                        Guid.NewGuid().ToString(),
                        Path.GetFileName(path),
                        path,
                        format.Value,
                        content
                    );
                    documents.Add(document);
                }
                return documents;
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "An error occurred while loading documents from folder {FolderPath}.", folderPath);
                return Enumerable.Empty<Document>();
            }
        }

        private static DocumentFormat? GetFormatFromExtension(string filePath) =>
            Path.GetExtension(filePath).ToLowerInvariant() switch
            {
                ".txt" => DocumentFormat.Text,
                ".md" => DocumentFormat.Markdown,
                _ => null
            };
    }
}
