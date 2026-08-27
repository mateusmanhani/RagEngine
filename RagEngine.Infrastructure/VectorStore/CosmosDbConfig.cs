namespace RagEngine.Infrastructure.VectorStore
{
    public class CosmosDbConfig
    {
        public string EndpointUri { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string ContainerName { get; set; } = string.Empty;
    }
}
