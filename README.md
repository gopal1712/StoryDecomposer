# StoryDecomposer

StoryDecomposer is an ASP.NET Core Web API that converts acceptance criteria into implementation tasks, generates story-refinement questions, and provides one common Fibonacci estimate for the complete story.

The API accepts only `acceptanceCriteria`. Story title and description are not required.

## Features

- Decomposes acceptance criteria into actionable tasks.
- Categorizes tasks by area such as UI, API, Database, Auth, DevOps, and Testing.
- Generates clarification questions for planning and refinement.
- Returns structured reasoning for the task breakdown.
- Estimates the complete story with one Fibonacci value: `1`, `2`, `3`, `5`, or `8`.
- Loads prompts from the `prompts` directory.
- Uses an in-memory vector store for related context.
- Supports Ollama or Groq for text generation.

## Architecture

1. The API receives acceptance criteria.
2. Ollama generates an embedding for the criteria.
3. The in-memory vector store searches for related context.
4. The configured LLM provider generates decomposition tasks.
5. The same provider generates planning questions.
6. The API returns tasks, questions, reasoning, and the common story estimate.

Ollama is currently required for embeddings, even when Groq is selected for text generation.

## Requirements

- .NET SDK 10.0 or later
- Ollama running locally for embeddings
- Ollama embedding model `phi3.5`
- Either Ollama generation model `tinyllama` or a Groq API account

## Configuration

Configuration is read from `appsettings.json` and environment-specific configuration files.

### Ollama generation

```json
{
  "LLM": {
    "Provider": "ollama"
  },
  "Ollama": {
    "BaseUrl": "http://localhost:11434",
    "Model": "tinyllama",
    "EmbeddingModel": "phi3.5",
    "TimeoutSeconds": 30
  }
}
```

Start Ollama and download the required models:

```bash
ollama serve
ollama pull tinyllama
ollama pull phi3.5
```

### Groq generation

Set the provider to `Groq` and provide the API key through an environment variable or .NET user secrets:

```json
{
  "LLM": {
    "Provider": "Groq"
  },
  "Groq": {
    "BaseUrl": "https://api.groq.com/openai/v1",
    "Model": "openai/gpt-oss-20b",
    "MaxTokens": 1024,
    "TimeoutSeconds": 30
  }
}
```

PowerShell:

```powershell
$env:Groq__ApiKey = "your-groq-api-key"
```

Command Prompt:

```cmd
set Groq__ApiKey=your-groq-api-key
```

Never commit a real API key to a public GitHub repository. If a key has already been committed, revoke it and create a replacement before publishing the repository.

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

To choose a specific URL:

```bash
dotnet run --urls http://localhost:5195
```

## API Usage

### Decompose a story

```http
POST /api/story/decompose
Content-Type: application/json
```

Request body:

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

## Output

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
    }
  ],
  "questions": [
    "Which payment gateway API and environment should be used?",
    "What validation rules are required for the card details?",
    "What security requirements apply to payment data?"
  ],
  "reasoning": [
    "The acceptance criteria were mapped to UI and API work.",
    "Error handling and testing risks influenced the estimate."
  ],
  "estimatedStoryPoints": 5
}
```

### Response fields

| Field | Type | Description |
| --- | --- | --- |
| `tasks` | array | Implementation tasks generated from the acceptance criteria. |
| `tasks[].title` | string | Short task name. |
| `tasks[].description` | string | Task details and technical scope. |
| `tasks[].areaOfChange` | string | Technical area affected by the task. |
| `questions` | array | Clarifying questions for story refinement. |
| `reasoning` | array | Explanation of the task breakdown and estimate. |
| `estimatedStoryPoints` | integer | One common Fibonacci estimate for the complete story. |

## Prompt Files

Prompt templates are stored in:

```text
prompts/task-decomposition-prompt.txt
prompts/questions-generation-prompt.txt
```

They are copied to the application output directory during build. Runtime placeholders include:

- `{acceptanceCriteria}`
- `{tasks}` in the questions prompt

Rebuild or restart the application after changing prompt files.

## Dependencies

### Runtime dependencies

- Ollama at `http://localhost:11434` for embeddings.
- Ollama model `phi3.5` for embeddings.
- Ollama generation model `tinyllama`, when `LLM:Provider` is `ollama`.
- Groq API access, when `LLM:Provider` is `Groq`.
- Groq model configured by `Groq:Model`, currently `openai/gpt-oss-20b`.

### NuGet dependencies

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
- There is no public endpoint for indexing documents into the vector store.
- Ollama is still required for embeddings when Groq is used for generation.
- The default HTTP timeout is 30 seconds; local model generation may require a larger value for slower machines.
- The API does not process real payments; payment-related output is planning guidance only.
- The API does not currently validate that `acceptanceCriteria` is non-empty.

## Build

```bash
dotnet build
```

The build may report a NuGet vulnerability warning for a transitive `Microsoft.OpenApi` package. Review package updates before deploying publicly.
