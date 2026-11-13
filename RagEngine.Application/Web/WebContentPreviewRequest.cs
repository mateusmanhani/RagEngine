using System.ComponentModel.DataAnnotations;

namespace RagEngine.Application.Web
{
    public sealed record WebContentPreviewRequest
    {
        [Required]
        [Url]
        public string Url { get; init; } = string.Empty;
    }
}
