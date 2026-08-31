using System.Text.Json.Serialization;

namespace RagEngine.Infrastructure.Config
{
    public class GenerateOptions
    {
        public double Temperature { get; init; } = 0.1;

        [JsonPropertyName("num_predict")]
        public int NumPredict { get; init; } = 200;
    }
}
