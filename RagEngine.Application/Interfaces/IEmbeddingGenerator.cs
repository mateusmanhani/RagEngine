namespace RagEngine.Application.Interfaces
{
    public interface IEmbeddingGenerator
    {
        Task<float[]> GenerateEmbeddingAsync(string input, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<float[]>> GenerateEmbeddingsAsync(IReadOnlyList<string> inputs, CancellationToken cancellationToken = default);
    }
}
