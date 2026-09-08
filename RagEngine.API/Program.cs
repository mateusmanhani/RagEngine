using Azure.Identity;
using Azure.Search.Documents;
using CommunityToolkit.VectorData.AzureCosmosDB;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DataIngestion;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;
using Microsoft.ML.Tokenizers;
using RagEngine.Application.Interfaces;
using RagEngine.Application.Services;
using RagEngine.Infrastructure.Config;
using RagEngine.Infrastructure.Cosmos;
using RagEngine.Infrastructure.Embedding;
using RagEngine.Infrastructure.Embedding.Ollama;
using RagEngine.Infrastructure.Synthesis;
using Scalar.AspNetCore;
using Serilog;
using System.Net.Http.Headers;
using System.Text.Json;
using VectorDistanceFunction = Microsoft.Extensions.VectorData.DistanceFunction;

namespace RagEngine
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            // ============================================================
            // LOGGING
            // ============================================================

            builder.Host.UseSerilog(
                (context, services, loggerConfiguration) =>
                    loggerConfiguration
                        .ReadFrom.Configuration(context.Configuration)
                        .ReadFrom.Services(services)
                        .Enrich.FromLogContext());


            // ============================================================
            // CONFIGURATION
            // ============================================================

            builder.Configuration.AddAzureKeyVault(
                new Uri(builder.Configuration["KeyVault:Uri"]!),
                new DefaultAzureCredential());


            builder.Services.Configure<ChunkingOptions>(
                builder.Configuration.GetSection("Chunking"));

            builder.Services.Configure<CosmosDbConfig>(
                builder.Configuration.GetSection("CosmosDb"));

            builder.Services.Configure<AzureSearchOptions>(
                builder.Configuration.GetSection("AzureSearch"));

            builder.Services.Configure<GroqOptions>(
                builder.Configuration.GetSection("Groq"));

            builder.Services.Configure<RagOptions>(
                builder.Configuration.GetSection("RagOptions"));

            builder.Services.Configure<GeminiOptions>(
                builder.Configuration.GetSection("Gemini"));

            builder.Services.Configure<OllamaOptions>(
                builder.Configuration.GetSection("Ollama"));


            // ============================================================
            // AZURE AI SEARCH
            // ============================================================

            builder.Services.AddSingleton<SearchClient>(serviceProvider =>
            {
                var configuration =
                    serviceProvider.GetRequiredService<IConfiguration>();

                var endpoint =
                    configuration["AzureSearch:Endpoint"];

                var indexName =
                    configuration["AzureSearch:IndexName"];

                return new SearchClient(
                    new Uri(endpoint!),
                    indexName!,
                    new AzureCliCredential());
            });


            // ============================================================
            // AI - EMBEDDINGS
            // ============================================================

            //builder.Services.AddHttpClient<IEmbeddingGenerator<string, Embedding<float>>,GeminiEmbeddingGenerator>(
            //    (serviceProvider, httpClient) =>
            //    {
            //        var options = serviceProvider.GetRequiredService<IOptions<GeminiOptions>>().Value;
            //        httpClient.BaseAddress = new Uri(options.BaseUrl);
            //        httpClient.DefaultRequestHeaders.Add("x-goog-api-key", options.ApiKey);
            //    });

            builder.Services.AddHttpClient<IEmbeddingGenerator<string, Embedding<float>>, OllamaEmbeddingGenerator>(
                 (serviceProvider, httpClient) =>
                 {
                     var options = serviceProvider
                         .GetRequiredService<IOptions<OllamaOptions>>()
                         .Value;

                     httpClient.BaseAddress = new Uri(options.BaseUrl);
                 });


            // ============================================================
            // AI - ANSWER GENERATION
            // ============================================================

            builder.Services.AddHttpClient<IAnswerGenerator,GroqAnswerGenerator>(
                (serviceProvider, httpClient) =>
                {
                    var options =serviceProvider.GetRequiredService<IOptions<GroqOptions>>().Value;

                    httpClient.BaseAddress =new Uri(options.BaseUrl);

                    httpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue(
                            "Bearer",
                            options.ApiKey);
                });


            // ============================================================
            // COSMOS DB
            // ============================================================

            builder.Services.AddSingleton<CosmosClient>(
                serviceProvider =>
                {
                    var options =
                        serviceProvider
                            .GetRequiredService<IOptions<CosmosDbConfig>>()
                            .Value;

                    var cosmosClientOptions = new CosmosClientOptions
                    {
                        UseSystemTextJsonSerializerWithOptions =
                            new JsonSerializerOptions(JsonSerializerDefaults.Web)
                    };

                    return new CosmosClient(
                        options.EndpointUri,
                        new DefaultAzureCredential(),
                        cosmosClientOptions);
                });


            // ============================================================
            // MICROSOFT VECTOR DATA - COSMOS VECTOR STORE
            // ============================================================

            builder.Services.AddSingleton<VectorStore>(serviceProvider =>
            {
                var cosmosClient =
                    serviceProvider.GetRequiredService<CosmosClient>();

                var cosmosOptions =
                    serviceProvider
                        .GetRequiredService<IOptions<CosmosDbConfig>>()
                        .Value;

                var embeddingGenerator =
                    serviceProvider.GetRequiredService<
                        IEmbeddingGenerator<string, Embedding<float>>>();

                var database =
                    cosmosClient.GetDatabase(
                        cosmosOptions.DatabaseName);

                return new CosmosVectorStore(
                    database,
                    new CosmosVectorStoreOptions
                    {
                        EmbeddingGenerator = embeddingGenerator
                    });
            });


            // ============================================================
            // DOCUMENT INGESTION - DOCUMENT READER
            // ============================================================

            builder.Services.AddSingleton<IngestionDocumentReader>(
                _ => new MarkItDownReader(
                    exePath: null,
                    extractImages: false));


            // ============================================================
            // DOCUMENT INGESTION - CHUNKER
            // ============================================================

            builder.Services.AddSingleton<IngestionChunker<string>>(
                serviceProvider =>
                {
                    var options =
                        serviceProvider
                            .GetRequiredService<IOptions<ChunkingOptions>>()
                            .Value;

                    var tokenizer =
                        TiktokenTokenizer.CreateForModel("gpt-4o");

                    var chunkerOptions =
                        new IngestionChunkerOptions(tokenizer)
                        {
                            MaxTokensPerChunk =
                                options.MaxTokensPerChunk,

                            OverlapTokens =
                                options.OverlapTokens
                        };

                    return new HeaderChunker(chunkerOptions);
                });


            // ============================================================
            // DOCUMENT INGESTION - VECTOR STORE WRITER
            // ============================================================

            builder.Services.AddSingleton<IngestionChunkWriter<string>>(
                serviceProvider =>
                {
                    var vectorStore =
                        serviceProvider
                            .GetRequiredService<VectorStore>();

                    var ollamaOptions =
                        serviceProvider
                            .GetRequiredService<IOptions<OllamaOptions>>()
                            .Value;

                    var cosmosOptions =
                        serviceProvider
                            .GetRequiredService<IOptions<CosmosDbConfig>>()
                            .Value;

                    return new VectorStoreWriter<string>(
                        vectorStore,
                        dimensionCount: ollamaOptions.EmbeddingDimensions,
                        options: new VectorStoreWriterOptions
                            {
                                CollectionName = cosmosOptions.ContainerName,
                                DistanceFunction = VectorDistanceFunction.CosineSimilarity,
                                IncrementalIngestion = true
                            });
                });


            // ============================================================
            // DOCUMENT INGESTION - PIPELINE
            // ============================================================

            builder.Services.AddSingleton<IngestionPipeline<string>>(
                serviceProvider =>
                {
                    var reader =
                        serviceProvider
                            .GetRequiredService<
                                IngestionDocumentReader>();

                    var chunker =
                        serviceProvider
                            .GetRequiredService<
                                IngestionChunker<string>>();

                    var writer =
                        serviceProvider
                            .GetRequiredService<
                                IngestionChunkWriter<string>>();

                    var loggerFactory =
                        serviceProvider
                            .GetRequiredService<
                                ILoggerFactory>();

                    return new IngestionPipeline<string>(
                        reader,
                        chunker,
                        writer,
                        loggerFactory: loggerFactory);
                });


            // ============================================================
            // APPLICATION SERVICES - INGESTION
            // ============================================================

            builder.Services.AddScoped<DocumentIngestionService>();


            // ============================================================
            // RETRIEVAL
            // ============================================================

            builder.Services.AddScoped<IRetriever, CosmosRetriever>();


            // ============================================================
            // RAG PIPELINE
            // ============================================================

            builder.Services.AddScoped<RagPipeline>();


            // ============================================================
            // API
            // ============================================================

            builder.Services.AddControllers();

            builder.Services.AddOpenApi();


            // ============================================================
            // APPLICATION
            // ============================================================

            var app = builder.Build();


            // ============================================================
            // DEVELOPMENT TOOLS
            // ============================================================

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();

                app.MapScalarApiReference();
            }


            // ============================================================
            // HTTP PIPELINE
            // ============================================================

            app.UseSerilogRequestLogging();

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}