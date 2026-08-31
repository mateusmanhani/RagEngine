using Microsoft.Extensions.Logging;
using RagEngine.Application.DTO;
using RagEngine.Application.Interfaces;

namespace RagEngine.Application.Services
{
    public class IngestionPipeline
    {
        private readonly ILogger<IngestionPipeline> _logger;
        private readonly IDocumentLoader _documentLoader;
        private readonly IChunker _chunker;
        private readonly IEmbeddingGenerator _embeddingGenerator;
        private readonly IVectorStore _vectorStore;

        public IngestionPipeline(
            ILogger<IngestionPipeline> logger,
            IDocumentLoader documentLoader, 
            IChunker chunker, 
            IEmbeddingGenerator embeddingGenerator, 
            IVectorStore vectorStore)
        {
            _logger = logger;
            _documentLoader = documentLoader;
            _chunker = chunker;
            _embeddingGenerator = embeddingGenerator;
            _vectorStore = vectorStore;
        }

        public async Task<IngestionResult> IngestFolderAsync (string folderPath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                _logger.LogError("Folder path cannot be null or empty.");
                return new IngestionResult(0, 0);
            }

            var documents = (await _documentLoader.LoadFromFolderAsync(folderPath, cancellationToken)).ToList();

            if (documents.Count == 0)
            {
                _logger.LogWarning("No valid documents found in the folder.");
                return new IngestionResult(0, 0);
            }

            var totalChunkCount = 0;

            foreach (var document in documents)
            {
                var chunks = _chunker.ChunkDocument(document).ToList();

                var embeddings = await _embeddingGenerator.GenerateEmbeddingsAsync(chunks.Select(c => c.Content).ToList(), cancellationToken);

                var embeddedChunks = chunks
                    .Zip(embeddings, (chunk, embedding) => chunk with { Embedding = embedding })
                    .ToList();

                await _vectorStore.AddAsync(embeddedChunks, cancellationToken);

                totalChunkCount += embeddedChunks.Count;
            }

            _logger.LogInformation(
                "Ingested {DocumentCount} documents into {ChunkCount} chunks from {FolderPath}.",
                documents.Count, totalChunkCount, folderPath);

            return new IngestionResult(documents.Count, totalChunkCount);
        }
    }
}
