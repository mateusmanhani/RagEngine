using RagEngine.Application.DTO;

namespace RagEngine.Application.Interfaces
{
    public interface IAnswerGenerator
    {
        Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default);
    }
}
