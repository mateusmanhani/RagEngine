using RagEngine.Domain.Entities;

namespace RagEngine.Application.Interfaces
{
    public interface IDocumentLoader
    {
        public Task<IEnumerable<Document>> LoadFromFolderAsync(string folderPath, CancellationToken cancellationToken = default);   
    }
}
