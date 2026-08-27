namespace RagEngine.Infrastructure.DocumentIngestion
{
    public class ChunkingOptions
    {
        public int MaxTokensPerLine { get; set; } = 100;
        public int MaxTokensPerParagraph { get; set; } = 300;
        public int OverlapTokens { get; set; } = 30;
    }
}
