namespace RagEngine.Infrastructure.Ollama.Synthesis
{
    public class GenerateResponse
    {
        public string Response { get; set; } = string.Empty;
        public bool Done { get; set; } = false;
    }
}
