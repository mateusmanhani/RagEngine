# RagEngine

A learning-focused **Retrieval-Augmented Generation (RAG) proof of concept** built with **C# / .NET 10**.

RagEngine explores how a RAG application works end-to-end: from document ingestion and chunking to embeddings, vector search, context retrieval, and LLM-generated answers.

The goal is not just to use AI tools, but to understand the architecture and trade-offs behind them.

## Architecture

```text
Documents (.txt, .md)
        ↓
Document Loading
        ↓
Chunking
        ↓
Embedding Generation
        ↓
Vector Storage
        ↓
Query
        ↓
Vector Similarity Search
        ↓
Relevant Context
        ↓
LLM Answer Generation
```

## Tech Stack

* **C# / .NET 10**
* **ASP.NET Core Web API**
* **Semantic Kernel** — document chunking
* **Ollama** — local embeddings and LLM inference

  * `qwen3-embedding:0.6b`
  * `qwen3:4b`
* **Azure Cosmos DB for NoSQL** — vector storage and similarity search
* **Scalar** — interactive API documentation
* **Options Pattern** — configurable RAG settings such as `TopK`

## Features

* Document ingestion from `.txt` and `.md` files
* Document chunking with Semantic Kernel
* Batch embedding generation
* Vector similarity search
* Configurable `TopK` retrieval
* Retrieval-Augmented Generation pipeline
* LLM answer generation using retrieved document context
* Diagnostics endpoints for inspecting stored chunks
* Clean architecture with interchangeable infrastructure components

## Project Structure

```text
RagEngine.API
    API controllers and application configuration

RagEngine.Application
    Application services, interfaces, and RAG pipelines

RagEngine.Domain
    Core domain entities

RagEngine.Infrastructure
    Embeddings, chunking, vector stores, and external integrations

Tests
    Unit and integration tests
```

The project uses abstractions at infrastructure boundaries to make components replaceable.

For example:

* `IEmbeddingGenerator` — Ollama today, another embedding provider later
* `IVectorStore` — different vector storage implementations
* LLM providers can be changed independently from retrieval

## Running Locally

### Prerequisites

* .NET 10 SDK
* [Ollama](https://ollama.com/) running locally

Pull the required models:

```powershell
ollama pull qwen3-embedding:0.6b
ollama pull qwen3:4b
```

### Run the API

```powershell
dotnet run --project RagEngine.API
```

Open the API documentation at:

```text
/scalar
```

## Example Workflow

### 1. Ingest documents

```http
POST /api/ingestion/folder?folderPath=C:\path\to\docs
```

The ingestion pipeline:

1. Loads supported documents
2. Splits them into chunks
3. Generates embeddings
4. Stores chunks and vectors

### 2. Ask a question

```http
GET /api/rag?query=Your question here
```

The RAG pipeline:

1. Generates an embedding for the query
2. Retrieves the most relevant document chunks
3. Builds contextual information for the LLM
4. Generates an answer based on the retrieved context

The number of retrieved chunks is configurable through `appsettings.json`:

```json
"RagOptions": {
  "TopK": 5
}
```

## Current Status

* ✅ Document ingestion
* ✅ Semantic chunking
* ✅ Local embedding generation with Ollama
* ✅ Batch embedding generation
* ✅ Vector similarity search
* ✅ Azure Cosmos DB vector store integration
* ✅ Retrieval pipeline
* ✅ Retrieval-Augmented Generation
* ✅ LLM answer generation
* ✅ Configurable retrieval settings
* 🚧 Hybrid search
* 🚧 Performance optimisation and model/provider evaluation
* 🚧 Prompt injection and additional security protections

## Why This Project?

This project is intentionally built incrementally.

Rather than using a framework to hide the entire RAG pipeline, each stage is implemented and explored individually to better understand:

* How document chunking affects retrieval
* How embeddings and semantic search work
* How vector databases perform similarity search
* How `TopK` affects context quality
* How retrieval impacts LLM responses
* Where performance bottlenecks occur
* The trade-offs between local and cloud-based models

## What's Next?

The project will continue evolving as new concepts are explored, including:

* Hybrid search
* Retrieval quality improvements
* Performance optimisation
* Alternative LLM providers
* Improved prompt and context handling
* Security and prompt-injection protections

---

**RagEngine is a learning project built to understand RAG architecture — not just to ship an AI application.**
