namespace RagEngine.Infrastructure.Config
{
    public sealed class FirecrawlOptions
    {
        public const string SectionName = "Firecrawl";

        public string BaseUrl { get; set; } = "https://api.firecrawl.dev";

        public string ApiKey { get; set; } = string.Empty;

        public int CrawlLimit { get; set; } = 20;

        public int PollIntervalSeconds { get; set; } = 2;

        public int MaxWaitMinutes { get; set; } = 5;
    }
}
