# RagEngine

A small, learning-focused Retrieval-Augmented Generation (RAG) proof of concept built with **C# / .NET 10**. RagEngine ingests local documents, chunks them, generates embeddings, stores them in a vector store, and exposes a browsable API for inspection — all running locally with [Ollama](https://ollama.com/).

This project is intentionally simple. It's a POC for exploring RAG architecture and Azure Cosmos DB vector search, not a production system.

## What it does

```text
Load documents (.txt, .md)
		↓
Chunk into passages (Semantic Kernel TextChunker)
		↓
Generate embeddings (Ollama, batched)
		↓
Store chunks + embeddings (vector store)
		↓
Inspect via API / Scalar
```

## Tech stack

- **.NET 10** / ASP.NET Core Web API
- **Ollama** (local) for embeddings — `qwen3-embedding:0.6b` (1024-dim)
- **Azure Cosmos DB for NoSQL** as the target vector store (vector search over `/embedding`)
- **Semantic Kernel** `TextChunker` for chunking
- **Scalar** for interactive API documentation

## Project structure

```text
RagEngine.API             API host: controllers, DI composition root
RagEngine.Application     Interfaces + orchestration (IngestionPipeline)
RagEngine.Domain          Core entities (Document, Chunk)
RagEngine.Infrastructure  Concrete implementations (Ollama, chunker, vector stores)
Tests                     Unit / integration tests
```

The design favors small interfaces at the boundaries so pieces can be swapped later — e.g. `IEmbeddingGenerator` (Ollama today, Azure OpenAI later) and `IVectorStore` (in-memory today, Cosmos DB when ready).

## Running locally

### Prerequisites

- .NET 10 SDK
- [Ollama](https://ollama.com/) running locally with the following models pulled:
  - `qwen3-embedding:0.6b`
  - `qwen3:4b`

### Run the API

```powershell
dotnet run --project RagEngine.API
```

Then browse the API reference at `/scalar`.

### Ingest a folder of documents

```http
POST /api/ingestion/folder?folderPath=C:\path\to\docs
```

Accepts a folder of `.txt` / `.md` files and returns a summary of documents and chunks processed.

### Inspect stored chunks

```http
GET /api/diagnostics/chunks
GET /api/diagnostics/chunks?documentId={id}
```

Useful for verifying chunk boundaries and embedding dimensions without querying the vector store directly.

### Generate an embedding manually

```http
POST /api/embeddings
```

## Current status

- ✅ Document loading, chunking, and batch embedding generation
- ✅ In-memory vector store with cosine similarity search
- ✅ Cosmos DB vector store implementation (built, not yet wired in)
- ✅ Ingestion pipeline + diagnostics endpoints
- 🚧 Retrieval-augmented answer generation (query → context → LLM) — not yet implemented
- 🚧 Prompt-injection protections and basic security — planned for a later phase

## Roadmap

This project follows an incremental plan: infrastructure integration → minimal vector store → basic RAG (retrieval + generation) → prompt-injection protections → basic security → optional agent/tool capabilities. See [`.github/copilot-instructions.md`](.github/copilot-instructions.md) for the full phased approach used to guide development.

## How this was built

This project was built with GitHub Copilot acting as a mentor/reviewer, not an autopilot: I wrote and reviewed the implementation myself, while Copilot was constrained by [`.github/copilot-instructions.md`](.github/copilot-instructions.md) to teach concepts, challenge design decisions, and enforce an incremental, POC-appropriate architecture rather than generating a finished app in one shot. See that file for the full guardrails.
