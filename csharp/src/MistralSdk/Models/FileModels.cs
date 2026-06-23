using System.Text.Json.Serialization;

namespace MistralSdk.Models;

public sealed class FileUploadResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("filename")]
    public string? FileName { get; init; }

    [JsonPropertyName("bytes")]
    public long? Bytes { get; init; }
}
