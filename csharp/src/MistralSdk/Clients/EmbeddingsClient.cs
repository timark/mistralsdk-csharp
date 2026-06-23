using MistralSdk.Models;

namespace MistralSdk.Clients;

public sealed class EmbeddingsClient
{
    private readonly MistralClient _client;

    internal EmbeddingsClient(MistralClient client)
    {
        _client = client;
    }

    public Task<EmbeddingResponse> CreateAsync(EmbeddingRequest request, CancellationToken cancellationToken = default)
        => _client.SendJsonAsync<EmbeddingRequest, EmbeddingResponse>(HttpMethod.Post, "/v1/embeddings", request, cancellationToken);
}
