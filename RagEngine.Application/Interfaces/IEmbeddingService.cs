namespace RagEngine.Application.Interfaces
{
    public interface IEmbeddingService
    {
        Task<float[]> GetEmbeddingAsync(string input, CancellationToken cancellationToken = default);
    }
}
