using System.Text.Json.Serialization;

namespace MistralSdk.Models;

public sealed class EmbeddingRequest
{
    [JsonPropertyName("model")]
    public required string Model { get; init; }

    [JsonPropertyName("input")]
    public required IReadOnlyList<string> Input { get; init; }
}

public sealed class EmbeddingResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("model")]
    public string? Model { get; init; }

    [JsonPropertyName("data")]
    public IReadOnlyList<EmbeddingItem>? Data { get; init; }
}

public sealed class EmbeddingItem
{
    [JsonPropertyName("index")]
    public int Index { get; init; }

    [JsonPropertyName("embedding")]
    public IReadOnlyList<float>? Embedding { get; init; }
}
