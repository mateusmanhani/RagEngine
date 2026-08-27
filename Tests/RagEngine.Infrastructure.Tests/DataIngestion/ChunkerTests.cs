using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using RagEngine.Domain.Entities;
using RagEngine.Domain.Enums;
using RagEngine.Infrastructure.DocumentIngestion;

namespace RagEngine.Infrastructure.Tests.DataIngestion
{
    public class ChunkerTests
    {
        private const string SampleText =
             "This is the first sentence of a sample document. " +
            "This is the second sentence, adding a bit more content. " +
            "Here is a third sentence to push the paragraph further. " +
            "And a fourth sentence, just to be safe about length. " +
            "Finally, a fifth sentence rounds out this sample paragraph.";

        private static SemanticKernelChunker CreateSut(ChunkingOptions options) =>
            new(NullLogger<SemanticKernelChunker>.Instance, Options.Create(options));

        [Fact]
        public void Chunkdocument_ShouldReturnMultipleChunks_WhenTextExceedsMaxTokensPerParagraph()
        {
            // Arrange
            var options = new ChunkingOptions
            {
                MaxTokensPerLine = 10,
                MaxTokensPerParagraph = 15,
                OverlapTokens = 2
            };
            var sut = CreateSut(options);
            var document = new Document(Guid.NewGuid(), "sample.txt", "sample.txt", DocumentFormat.Text, SampleText);

            // Act
            var chunks = sut.ChunkDocument(document).ToList();

            // Assert
            chunks.Should().HaveCountGreaterThan(1);
        }

        [Fact]
        public void Chunkdocument_ShouldReturnSequentialChunkIndexes()
        {
            // Arrange
            var options = new ChunkingOptions
            {
                MaxTokensPerLine = 10,
                MaxTokensPerParagraph = 15,
                OverlapTokens = 2
            };
            var sut = CreateSut(options);
            var document = new Document(Guid.NewGuid(), "sample.txt", "sample.txt", DocumentFormat.Text, SampleText);

            // Act
            var chunks = sut.ChunkDocument(document).ToList();

            // Assert
            chunks.Select(c => c.ChunkIndex).Should().BeInAscendingOrder();
        }

        [Fact]
        public void Chunkdocument_ShouldReturnEmpty_WhenDocumentIsNull()
        {
            // Arrange
            var options = new ChunkingOptions
            {
                MaxTokensPerLine = 10,
                MaxTokensPerParagraph = 15,
                OverlapTokens = 2
            };
            var sut = CreateSut(options);

            // Act
            var chunks = sut.ChunkDocument(null).ToList();

            // Assert
            chunks.Should().BeEmpty();
        }

        [Fact]
        public void Chunkdocument_ShouldSetDocumentIdOnEveryChunk()
        {
            // Arrange
            var options = new ChunkingOptions
            {
                MaxTokensPerLine = 10,
                MaxTokensPerParagraph = 15,
                OverlapTokens = 2
            };
            var sut = CreateSut(options);
            var document = new Document(Guid.NewGuid(), "sample.txt", "sample.txt", DocumentFormat.Text, SampleText);

            // Act
            var chunks = sut.ChunkDocument(document).ToList();

            // Assert
            chunks.Select(c => c.DocumentId).Should().AllBeEquivalentTo(document.Id.ToString());
        }
    }
}
