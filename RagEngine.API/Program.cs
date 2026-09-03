
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
using RagEngine.Infrastructure;
using Azure.Search.Documents;
using Azure.Identity;
using System.Net.Http.Headers;

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

            // -------- Configuration --------
            builder.Configuration.AddAzureKeyVault(new Uri(builder.Configuration["KeyVault:Uri"]!), new DefaultAzureCredential());

            builder.Services.Configure<OllamaOptions>(builder.Configuration.GetSection("Ollama"));
            builder.Services.Configure<CosmosDbConfig>(builder.Configuration.GetSection("CosmosDb"));
            builder.Services.Configure<ChunkingOptions>(builder.Configuration.GetSection("Chunking"));
            builder.Services.Configure<AzureSearchOptions>(builder.Configuration.GetSection("AzureSearch"));
            builder.Services.Configure<GroqOptions>(builder.Configuration.GetSection("Groq"));
            builder.Services.Configure<RagOptions>(builder.Configuration.GetSection("RagOptions"));

            builder.Services.AddSingleton<SearchClient>(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();

                var endpoint = config["AzureSearch:Endpoint"];
                var indexName = config["AzureSearch:IndexName"];

                return new SearchClient(
                    new Uri(endpoint!),
                    indexName!,
                    new AzureCliCredential());
            });

            // --------- AI --------

            builder.Services.AddHttpClient<IEmbeddingGenerator, OllamaEmbeddingGenerator>(
                (serviceProvider, httpClient) =>
            {
                var options = serviceProvider
                .GetRequiredService<IOptions<OllamaOptions>>().Value;

                httpClient.BaseAddress = new Uri(options.BaseUrl);
            });

            builder.Services.AddHttpClient<IAnswerGenerator, GroqAnswerGenerator>(
                (serviceProvider, httpClient) =>
            {
                var options = serviceProvider
                .GetRequiredService<IOptions<GroqOptions>>().Value;
                httpClient.BaseAddress = new Uri(options.BaseUrl);
                httpClient.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue(
                        "Bearer", options.ApiKey);
            });

            // -------------  Ingestion ------------------

            builder.Services.AddScoped<IChunker, SemanticKernelChunker>();

            builder.Services.AddScoped<IDocumentLoader, DocumentLoader>();
            
            builder.Services.AddScoped<IVectorStore, CosmosDBVectorStore>();

            //    -------------     Pipelines      ------------------

            builder.Services.AddScoped<IngestionPipeline>();
            builder.Services.AddScoped<IRetriever, AzureSearchRetriever>();
            builder.Services.AddScoped<RagPipeline>();

            //  -------------     API      ------------------

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
