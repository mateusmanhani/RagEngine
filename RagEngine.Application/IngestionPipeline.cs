using Microsoft.Extensions.Logging;
using RagEngine.Application.Interfaces;
using RagEngine.Domain.Entities;

namespace RagEngine.Application
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

        public async Task IngestFolderAsync (string folderPath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                _logger.LogError("Folder path cannot be null or empty.");
                return;
            }

            var documents = (await _documentLoader.LoadFromFolderAsync(folderPath, cancellationToken)).ToList();

            if (documents.Count == 0)
            {
                _logger.LogWarning("No valid documents found in the folder.");
                return;
            }

            foreach (var document in documents)
            {
                var chunks = _chunker.ChunkDocument(document).ToList();

                var embeddings = await _embeddingGenerator.GenerateEmbeddingsAsync(chunks.Select(c => c.Content).ToList(), cancellationToken);

                var embeddedChunks = chunks
                    .Zip(embeddings, (chunk, embedding) => chunk with { Embedding = embedding })
                    .ToList();

                await _vectorStore.AddAsync(embeddedChunks, cancellationToken);
            }
        }
    }
}
