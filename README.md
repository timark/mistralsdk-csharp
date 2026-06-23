# Mistral C# SDK (.NET 10)

This repository now contains a C# SDK for the Mistral AI API, targeting **.NET 10**.

## Requirements

- .NET SDK 10.0+
- A Mistral API key (`MISTRAL_API_KEY`)

## Project layout

- `/csharp/src/MistralSdk` - SDK library
- `/csharp/tests/MistralSdk.Tests` - unit tests
- `/MistralSdk.slnx` - solution file

## Build

```bash
dotnet build /home/runner/work/mistralsdk-csharp/mistralsdk-csharp/MistralSdk.slnx
```

## Test

```bash
dotnet test /home/runner/work/mistralsdk-csharp/mistralsdk-csharp/MistralSdk.slnx
```

## Usage

### Initialize client

```csharp
using MistralSdk;

var client = new MistralClient(new MistralClientOptions
{
    ApiKey = Environment.GetEnvironmentVariable("MISTRAL_API_KEY") ?? "",
});
```

### Chat completion

```csharp
using MistralSdk.Models;

var response = await client.Chat.CompleteAsync(new ChatCompletionRequest
{
    Model = "mistral-large-latest",
    Messages =
    [
        new ChatMessage
        {
            Role = "user",
            Content = "Who is the best French painter? Answer in one sentence.",
        },
    ],
});

Console.WriteLine(response.Choices?[0].Message?.Content);
```

### Streaming (SSE)

```csharp
await foreach (var chunk in client.Chat.StreamAsync(new ChatCompletionRequest
{
    Model = "mistral-large-latest",
    Stream = true,
    Messages =
    [
        new ChatMessage { Role = "user", Content = "Give me a short poem." },
    ],
}))
{
    Console.WriteLine(chunk);
}
```

### Embeddings

```csharp
var embeddings = await client.Embeddings.CreateAsync(new EmbeddingRequest
{
    Model = "mistral-embed",
    Input = ["hello world", "how are you"],
});

Console.WriteLine(embeddings.Data?.Count);
```

### File upload

```csharp
await using var stream = File.OpenRead("example.txt");
var uploaded = await client.Files.UploadAsync(stream, "example.txt", purpose: "assistants");

Console.WriteLine(uploaded.Id);
```

## Configuring retries and timeout

```csharp
var client = new MistralClient(new MistralClientOptions
{
    ApiKey = Environment.GetEnvironmentVariable("MISTRAL_API_KEY") ?? "",
    Timeout = TimeSpan.FromSeconds(30),
    MaxRetryAttempts = 3,
    RetryBaseDelay = TimeSpan.FromMilliseconds(250),
});
```
