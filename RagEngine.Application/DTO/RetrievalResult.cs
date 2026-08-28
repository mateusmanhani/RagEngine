using RagEngine.Domain.Entities;

namespace RagEngine.Application.DTO
{
    public record RetrievalResult(Chunk Chunk, double SimilarityScore);
}
