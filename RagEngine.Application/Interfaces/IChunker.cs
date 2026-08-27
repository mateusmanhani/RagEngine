using RagEngine.Domain.Entities;

namespace RagEngine.Application.Interfaces
{
    public interface IChunker
    {
        IEnumerable<Chunk> ChunkDocument(Document document);
    }
}
