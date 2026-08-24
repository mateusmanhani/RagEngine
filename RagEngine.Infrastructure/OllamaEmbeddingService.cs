using RagEngine.Application;

namespace RagEngine.Infrastructure
{
    internal class OllamaEmbeddingService : IEmbeddingService
    {
        public async Task<float[]> GetEmbeddingAsync(string input, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
