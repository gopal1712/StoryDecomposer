# StoryDecomposer

StoryDecomposer is an ASP.NET Core Web API that converts acceptance criteria into implementation tasks, generates story-refinement questions, and estimates the complete story using Fibonacci story points.

The application uses a local Ollama model for generation and embeddings. Story title and description are not required: the only story input is `acceptanceCriteria`.

## Features

- Decomposes acceptance criteria into actionable tasks.
- Categorizes tasks by area such as UI, API, Database, Auth, DevOps, and Testing.
- Generates planning and refinement questions.
- Returns structured reasoning for the task breakdown.
- Estimates the complete story using one Fibonacci value: `1`, `2`, `3`, `5`, or `8`.
- Loads prompts from the `prompts` directory and copies them to the build output.
- Includes an in-memory RAG/vector-search layer for related context.

## Requirements

- .NET SDK 10.0 or later
- Ollama running locally
- Ollama model `phi`
- Windows, Linux, or macOS

Install and start Ollama, then make sure the configured model is available:

```bash
ollama pull phi
ollama serve
```

Ollama must be reachable at:

```text
http://localhost:11434
```

The model name is currently configured in `Services/OllamaService.cs` and `RAG/EmbeddingService.cs`.

## Run Locally

From the repository root:

```bash
dotnet restore
dotnet build
dotnet run
```

The default HTTP URL is:

```text
http://localhost:5195
```

The HTTPS profile uses:

```text
https://localhost:7213
```

For local API testing, HTTP is usually the simplest option:

```bash
dotnet run --urls http://localhost:5195
```

## API Usage

### Decompose a Story

```http
POST /api/story/decompose
Content-Type: application/json
```

Example request:

```json
{
  "acceptanceCriteria": "User can enter card details. Payment is processed successfully. Error handling for failed payments."
}
```

Using `curl`:

```bash
curl -X POST http://localhost:5195/api/story/decompose \
  -H "Content-Type: application/json" \
  -d '{
    "acceptanceCriteria": "User can enter card details. Payment is processed successfully. Error handling for failed payments."
  }'
```

The API performs these operations:

1. Creates an embedding for the acceptance criteria through Ollama.
2. Searches the in-memory vector store for related context.
3. Generates task decomposition JSON through Ollama.
4. Generates planning questions through Ollama.
5. Returns the combined result.

## Response

Example response:

```json
{
  "tasks": [
    {
      "title": "Implement card details input UI",
      "description": "Create a user interface that allows users to enter card details.",
      "areaOfChange": "UI"
    },
    {
      "title": "Integrate payment processing API",
      "description": "Connect the application to the payment gateway and handle successful and failed payment responses.",
      "areaOfChange": "API"
    },
    {
      "title": "Test payment scenarios",
      "description": "Test successful payments, invalid card details, and failed payment handling.",
      "areaOfChange": "Testing"
    }
  ],
  "questions": [
    "What validation rules are required for the card details?",
    "Which payment gateway API and environment should be used?",
    "How should failed payments be communicated to the user?",
    "What security requirements apply to payment data?",
    "Is the Fibonacci estimate appropriate given the integration risks?"
  ],
  "reasoning": [
    "The acceptance criteria were mapped to UI, API, and testing work.",
    "Payment processing was separated from validation and error handling because they have different technical risks."
  ],
  "estimatedStoryPoints": 5
}
```

### Response Fields

| Field | Type | Description |
| --- | --- | --- |
| `tasks` | array | Implementation tasks generated from the acceptance criteria. |
| `tasks[].title` | string | Short task name. |
| `tasks[].description` | string | Task details. |
| `tasks[].areaOfChange` | string | Technical area affected by the task. |
| `questions` | array | Clarifying questions for story refinement. |
| `reasoning` | array | Explanation of the decomposition and estimate. |
| `estimatedStoryPoints` | integer | One common Fibonacci estimate for the complete story. |

## Prompt Files

Prompt templates are stored in:

```text
prompts/task-decomposition-prompt.txt
prompts/questions-generation-prompt.txt
```

They are copied to the application output directory during build. The services replace these placeholders at runtime:

- `{acceptanceCriteria}`
- `{tasks}` in the questions prompt

When editing prompts, rebuild or restart the application so the updated files are copied to the output directory.

## Dependencies

### Runtime Dependencies

- Ollama at `http://localhost:11434`
- Ollama model `phi`
- Ollama `/api/generate` endpoint for task and question generation
- Ollama `/api/embeddings` endpoint for RAG queries

### NuGet Dependencies

Defined in `StoryDecomposer.csproj`:

- `LangChain` `0.17.1`
- `Microsoft.AspNetCore.OpenApi` `10.0.6`
- `Microsoft.SemanticKernel` `1.80.1`
- `Newtonsoft.Json` `13.0.4`

## Project Structure

```text
Controllers/       HTTP API controllers
Models/            Request and response models
Services/          LLM, RAG, decomposition, and question services
RAG/               Embedding and in-memory vector-store code
prompts/           Prompt templates used by the services
Program.cs         Dependency injection and application startup
```

## Current Limitations

- The vector store is in-memory and loses its data when the application stops.
- There is currently no public API endpoint for indexing documents into the vector store.
- Task generation and question generation depend on Ollama response time.
- The default Ollama client timeout is five minutes.
- The API does not process real payments; payment-related output is planning guidance only.

## Build

```bash
dotnet build
```

The build may report a NuGet vulnerability warning for the transitive `Microsoft.OpenApi` package. Treat that warning separately from compilation errors and review package updates before deploying publicly.
