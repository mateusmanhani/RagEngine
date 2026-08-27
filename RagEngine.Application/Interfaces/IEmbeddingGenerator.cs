namespace RagEngine.Application.Interfaces
{
    public interface IEmbeddingGenerator
    {
        Task<float[]> GenerateEmbeddingAsync(string input, CancellationToken cancellationToken = default);
    }
}
