using RagEngine.Domain.Enums;

namespace RagEngine.Domain.Entities
{
    public record Document(
    string Id,
    string SourceName,
    string FilePath,
    DocumentFormat Format,
    string Content);
}
