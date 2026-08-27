using Microsoft.Extensions.Logging;
using RagEngine.Application.Interfaces;
using RagEngine.Domain.Entities;
using RagEngine.Domain.Enums;
using Microsoft.SemanticKernel.Text;
using Microsoft.Extensions.Options;

namespace RagEngine.Infrastructure.DocumentIngestion
{
    public class SemanticKernelChunker : IChunker
    {
        private readonly ILogger<SemanticKernelChunker> _logger;
        private readonly ChunkingOptions _options;

        public SemanticKernelChunker(ILogger<SemanticKernelChunker> logger, IOptions<ChunkingOptions> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        public IEnumerable<Chunk> ChunkDocument(Document document)
        {
            if (document == null) 
            { 
                _logger.LogWarning("Document is null. Cannot chunk a null document.");
                return Enumerable.Empty<Chunk>(); 
            }

#pragma warning disable SKEXP0050 // TextChunker is experimental; acceptable for this POC, revisit if it changes.
            var lines = document.Format switch 
            {
                DocumentFormat.Text => TextChunker.SplitPlainTextLines(document.Content, _options.MaxTokensPerLine),
                DocumentFormat.Markdown => TextChunker.SplitMarkDownLines(document.Content, _options.MaxTokensPerLine),
                _ => throw new NotSupportedException($"Document format {document.Format} is not supported.")
            };

#pragma warning disable SKEXP0050 // TextChunker is experimental; acceptable for this POC, revisit if it changes.
            var paragraphs = document.Format switch
            {
                DocumentFormat.Text => TextChunker.SplitPlainTextParagraphs(lines, _options.MaxTokensPerParagraph),
                DocumentFormat.Markdown => TextChunker.SplitMarkdownParagraphs(lines, _options.MaxTokensPerParagraph),
                _ => throw new NotSupportedException($"Document format {document.Format} is not supported.")
            };

            return paragraphs.Select((content, index) => new Chunk(
                Id: $"{document.Id}-{index}",
                DocumentId: document.Id.ToString(),
                ChunkIndex: index,
                Content: content));


        }
    }
}
