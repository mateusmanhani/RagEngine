
using Microsoft.Extensions.Options;
using Serilog;
using RagEngine.Application.Interfaces;
using RagEngine.Application.Services;
using RagEngine.Infrastructure.DocumentIngestion;
using RagEngine.Infrastructure.Config;
using RagEngine.Infrastructure.VectorStore;
using Scalar.AspNetCore;
using RagEngine.Infrastructure.Synthesis;
using RagEngine.Infrastructure.Embedding;

namespace RagEngine
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext());

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

            builder.Services.AddHttpClient<IAnswerGenerator, OllamaAnswerGenerator>(
                (serviceProvider, httpClient) =>
            {
                var options = serviceProvider
                .GetRequiredService<IOptions<OllamaOptions>>().Value;
                httpClient.BaseAddress = new Uri(options.BaseUrl);
            });

            builder.Services.Configure<ChunkingOptions>(builder.Configuration.GetSection("Chunking"));
            builder.Services.AddScoped<IChunker, SemanticKernelChunker>();

            builder.Services.AddScoped<IDocumentLoader, DocumentLoader>();

            //builder.Services.AddSingleton<IVectorStore, InMemoryVectorStore>();
            builder.Services.Configure<CosmosDbConfig>(builder.Configuration.GetSection("CosmosDb"));
            builder.Services.AddScoped<IVectorStore, CosmosDBVectorStore>();

            builder.Services.AddScoped<IngestionPipeline>();
            builder.Services.AddScoped<RetrievalPipeline>();
            builder.Services.AddScoped<RagPipeline>();

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            app.UseSerilogRequestLogging();
            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
