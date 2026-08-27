using RagEngine.Domain.Enums;

namespace RagEngine.Domain.Entities
{
    public record Document(
    Guid Id,
    string SourceName,
    string FilePath,
    DocumentFormat Format,
    string Content);
}
