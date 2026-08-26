namespace RagEngine.Application.Interfaces
{
    public interface IEmbeddingService
    {
        Task<float[]> GenerateEmbeddingAsync(string input, CancellationToken cancellationToken = default);
    }
}
