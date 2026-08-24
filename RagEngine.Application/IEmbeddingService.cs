namespace RagEngine.Application
{
    public interface IEmbeddingService
    {
        Task<float[]> GetEmbeddingAsync(string input, CancellationToken cancellationToken = default);
    }
}
