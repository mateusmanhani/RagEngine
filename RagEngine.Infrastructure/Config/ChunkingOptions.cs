namespace RagEngine.Infrastructure.Config
{
    public class ChunkingOptions
    {
        public int MaxTokensPerChunk { get; set; } = 500;
        public int OverlapTokens { get; set; } = 50;
        public float SemanticSimilarityThresholdPercentile { get; set; } = 95;
    }
}
