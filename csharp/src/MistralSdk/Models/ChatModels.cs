using System.Text.Json.Serialization;

namespace MistralSdk.Models;

public sealed class ChatCompletionRequest
{
    [JsonPropertyName("model")]
    public required string Model { get; init; }

    [JsonPropertyName("messages")]
    public required IReadOnlyList<ChatMessage> Messages { get; init; }

    [JsonPropertyName("stream")]
    public bool? Stream { get; init; }
}

public sealed class ChatMessage
{
    [JsonPropertyName("role")]
    public required string Role { get; init; }

    [JsonPropertyName("content")]
    public required string Content { get; init; }
}

public sealed class ChatCompletionResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("model")]
    public string? Model { get; init; }

    [JsonPropertyName("created")]
    public long? Created { get; init; }

    [JsonPropertyName("choices")]
    public IReadOnlyList<ChatChoice>? Choices { get; init; }
}

public sealed class ChatChoice
{
    [JsonPropertyName("index")]
    public int Index { get; init; }

    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; init; }

    [JsonPropertyName("message")]
    public ChatMessage? Message { get; init; }
}
