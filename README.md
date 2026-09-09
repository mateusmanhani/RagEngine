# RagEngine

A learning-focused **Retrieval-Augmented Generation (RAG) proof of concept** built with **C# / .NET 10**.

RagEngine explores RAG end-to-end: document parsing, chunking, embeddings, vector storage, similarity retrieval, context construction, and LLM answer generation.

The project also includes an alternative **Azure AI Search** implementation so the trade-offs between a custom RAG pipeline and a managed Azure retrieval pipeline can be explored.

## Architecture

### Cosmos DB RAG pipeline

```text
PDF
 ↓
MarkItDown
 ↓
Microsoft.Extensions.DataIngestion
 ↓
IngestionChunker + tokenizer
(HeaderChunker)
 ↓
IngestionChunkWriter
 ↓
IEmbeddingGenerator
 ├── Ollama
 └── Gemini API
 ↓
VectorStore / VectorStoreWriter
 ↓
Azure Cosmos DB
 ↓
CosmosRetriever
(VectorStoreCollection similarity search)
 ↓
Retrieved context
 ↓
Answer Generator
(GROQ)
 ↓
Answer
```

The ingestion flow uses Microsoft's **DataIngestion** abstractions rather than a custom ingestion pipeline implementation.

### Azure AI Search alternative

```text
PDF
 ↓
Azure Blob Storage
 ↓
Azure AI Search
 ├── Chunking
 ├── Embedding/indexing
 └── Retrieval
 ↓
Similarity search
 ↓
Answer Generator
(GROQ)
 ↓
Answer
```

In this approach, Azure AI Search manages document processing, chunking, indexing, and retrieval. RagEngine delegates answer synthesis to the same answer-generation component used by the Cosmos-based pipeline.

## Tech Stack

- **C# / .NET 10**
- **ASP.NET Core Web API**
- **Microsoft.Extensions.VectorData.Abstractions** — vector store abstractions
- **Microsoft.Extensions.DataIngestion** — document ingestion and chunking pipeline
- **MarkItDown** — PDF-to-markdown/document extraction
- **Microsoft.Extensions.AI** — `IEmbeddingGenerator` abstraction
- **Ollama** — local embedding generation
- **Gemini API** — alternative cloud embedding provider
- **Azure Cosmos DB for NoSQL** — vector storage and similarity search
- **Azure AI Search** — alternative managed search/retrieval implementation
- **GROQ** — answer generation
- **Scalar** — interactive API documentation
- **Options Pattern** — configurable RAG settings such as `TopK`

## Key Features

- PDF document ingestion using MarkItDown
- Microsoft DataIngestion pipeline for document processing
- Header-aware chunking using Microsoft's ingestion chunker and tokenizer
- Chunk writing through `IngestionChunkWriter`
- Embedding generation through Microsoft's `IEmbeddingGenerator`
- Interchangeable Ollama and Gemini embedding providers
- Vector storage through `VectorStore` / `VectorStoreWriter`
- Azure Cosmos DB similarity search through `VectorStoreCollection`
- Configurable `TopK` retrieval
- GROQ-based answer generation using retrieved context
- Alternative Azure AI Search pipeline with managed chunking, indexing, and retrieval
- Diagnostics for inspecting retrieval/chunking behaviour

## Project Structure

The project uses a modular application structure with abstractions around external infrastructure.

```text
RagEngine
├── API
│   └── Controllers and API configuration
├── Application
│   └── RAG, ingestion, retrieval, and answer-generation logic
├── Domain
│   └── Core models
└── Infrastructure
    └── Data ingestion, embeddings, vector stores,
        Azure AI Search, Ollama, Gemini, and GROQ integrations
```

The exact structure may evolve as the project develops.

## Running Locally

### Prerequisites

- .NET 10 SDK
- Ollama, if using the local embedding provider
- Azure Cosmos DB, if using the Cosmos vector pipeline
- Azure AI Search and Blob Storage, if using the Azure AI Search pipeline
- Gemini API credentials, if using Gemini embeddings
- GROQ API credentials for answer generation

Configuration and secrets should be supplied through standard .NET configuration mechanisms and should not be committed to source control.

### Run the API

```powershell
dotnet run
```

Open the API documentation at:

```text
/scalar
```

## Example RAG Workflow

### Cosmos DB pipeline

1. PDF is parsed with MarkItDown.
2. Microsoft's DataIngestion pipeline processes the document.
3. The document is split into header-aware chunks using a tokenizer.
4. Chunks are written using `IngestionChunkWriter`.
5. Embeddings are generated through `IEmbeddingGenerator`.
6. Chunks and vectors are written to the vector store.
7. `CosmosRetriever` performs vector similarity search.
8. Retrieved chunks are passed to the GROQ-based Answer Generator.
9. The generated answer is returned to the user.

### Azure AI Search pipeline

1. Documents are uploaded to Blob Storage.
2. Azure AI Search processes and indexes the documents.
3. Azure AI Search performs chunking, embedding/indexing, and retrieval.
4. Relevant results are returned to RagEngine.
5. The same Answer Generator synthesizes the final answer using the retrieved context.

## Current Status

- ✅ PDF document extraction with MarkItDown
- ✅ Microsoft DataIngestion pipeline
- ✅ Header-aware chunking with tokenizer
- ✅ `IngestionChunkWriter`
- ✅ `IEmbeddingGenerator` abstraction
- ✅ Ollama embeddings
- ✅ Gemini API embeddings
- ✅ Cosmos DB vector storage
- ✅ `VectorStore` / `VectorStoreWriter`
- ✅ Cosmos similarity retrieval with `VectorStoreCollection`
- ✅ GROQ answer generation
- ✅ Azure AI Search alternative pipeline
- ✅ Managed Azure AI Search chunking/indexing/retrieval tested
- 🚧 Retrieval and model/provider evaluation
- 🚧 Performance optimisation
- 🚧 Additional prompt-injection and security protections

## Why This Project?

RagEngine is intentionally built to understand the architecture behind RAG rather than hiding it behind a single framework.

The project is used to explore:

- How document parsing and chunking affect retrieval quality
- How tokenizers and chunking strategies affect context
- How embedding providers can be swapped through standard Microsoft abstractions
- How vector stores perform similarity search
- How managed search services compare with application-controlled RAG pipelines
- How retrieval quality affects generated answers
- The trade-offs between local, cloud, and managed Azure services
- Performance, cost, complexity, and maintainability

## What's Next?

The project will continue evolving around:

- Retrieval quality evaluation
- Chunking and embedding experiments
- Performance optimisation
- Provider/model comparison
- Prompt and context improvements
- Security and prompt-injection protections
- Further comparison between Cosmos DB and Azure AI Search

---

**RagEngine is a learning project built to understand RAG architecture — not just to ship an AI application.**
