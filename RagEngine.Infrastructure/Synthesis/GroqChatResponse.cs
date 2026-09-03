namespace RagEngine.Infrastructure.Synthesis
{
    public class GroqChatResponse
    {
        public List<GroqChoice> Choices { get; set; } = [];
        public GroqUsage? Usage { get; set; }
    }

    public class GroqChoice
    {
        public GroqMessage Message { get; set; } = new();
    }
    public class GroqUsage
    {
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens { get; set; }
    }
}
