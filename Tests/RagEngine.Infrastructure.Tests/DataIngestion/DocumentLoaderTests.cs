using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using RagEngine.Infrastructure.DocumentIngestion;

namespace RagEngine.Infrastructure.Tests.DataIngestion
{
    public class DocumentLoaderTests : IDisposable
    {
        private readonly string _tempFolder;
        private readonly DocumentLoader _sut; //system under test
        
        public DocumentLoaderTests()
        {
            _tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempFolder);
            _sut = new DocumentLoader(NullLogger<DocumentLoader>.Instance);
        }
        
        [Fact]
        public async Task LoadFromFolderAsync_ShouldLoadDocumentsCorrectly()
        {
            // Arrange
            await File.WriteAllTextAsync(Path.Combine(_tempFolder, "notes.txt"), "hello world", CancellationToken.None);
            await File.WriteAllTextAsync(Path.Combine(_tempFolder, "readme.md"), "# heading", CancellationToken.None);

            // Act
            var documents = await _sut.LoadFromFolderAsync(_tempFolder, CancellationToken.None);

            // Assert
            documents.Should().HaveCount(2);
            documents.Should().Contain(d => d.SourceName == "notes.txt" && d.Content == "hello world");
        }

        [Fact]
        public async Task LoadFromFolderAsync_ShouldHandleEmptyFilesGracefully()
        {
            // Arrange
            await File.WriteAllTextAsync(Path.Combine(_tempFolder, "notes.txt"), "", CancellationToken.None);
            await File.WriteAllTextAsync(Path.Combine(_tempFolder, "readme.md"), "          ", CancellationToken.None);

            // Act
            var documents = await _sut.LoadFromFolderAsync(_tempFolder, CancellationToken.None);

            // Assert
            documents.Should().BeEmpty();
        }

        [Fact]
        public async Task LoadFromFolderAsync_ShouldHandleUnsupportedFileTypesGracefully()
        {
            // Arrange
            await File.WriteAllTextAsync(Path.Combine(_tempFolder, "notes.pdf"), " This is  a pdf file", CancellationToken.None);
            await File.WriteAllTextAsync(Path.Combine(_tempFolder, "readme.xml"), " <! dcwsdc>  dsd", CancellationToken.None);

            // Act
            var documents = await _sut.LoadFromFolderAsync(_tempFolder, CancellationToken.None);

            // Assert
            documents.Should().BeEmpty();
        }

        [Fact]
        public async Task LoadFromFolderAsync_ShouldHandleExceptionsGracefully()
        {
            // Arrange
            var nonExistentFolder = Path.Combine(_tempFolder, "does-not-exist");

            // Act
            var act = async () => await _sut.LoadFromFolderAsync(nonExistentFolder, CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
            var documents = await _sut.LoadFromFolderAsync(nonExistentFolder, CancellationToken.None);
            documents.Should().BeEmpty();

        }

        public void Dispose()
        {
            Directory.Delete(_tempFolder, recursive: true);
        }
    }
}
