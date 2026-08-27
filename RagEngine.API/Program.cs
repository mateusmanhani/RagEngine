
using Microsoft.Extensions.Options;
using RagEngine.Application.Interfaces;
using RagEngine.Infrastructure.DocumentIngestion;
using RagEngine.Infrastructure.Embedding;
using RagEngine.Infrastructure.VectorStore;

namespace RagEngine
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.Configure<OllamaOptions>(
                builder.Configuration.GetSection("Ollama"));

            builder.Services.AddHttpClient<IEmbeddingGenerator, OllamaEmbeddingGenerator> (
                (serviceProvider, httpClient) =>
            {
                var options = serviceProvider
                .GetRequiredService<IOptions<OllamaOptions>>().Value;

                httpClient.BaseAddress = new Uri(options.BaseUrl);
            });

            builder.Services.Configure<ChunkingOptions>(builder.Configuration.GetSection("Chunking"));
            builder.Services.AddScoped<IChunker, SemanticKernelChunker>();

            builder.Services.AddScoped<IDocumentLoader, DocumentLoader>();

            builder.Services.AddSingleton<IVectorStore, InMemoryVectorStore>();

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
