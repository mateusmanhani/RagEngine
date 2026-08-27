using RagEngine.Domain.Entities;

namespace RagEngine.Application
{
    public record RetrievalResult(Chunk Chunk, double SimilarityScore);
}
