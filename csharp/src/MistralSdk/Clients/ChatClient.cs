using System.Runtime.CompilerServices;
using MistralSdk.Models;

namespace MistralSdk.Clients;

public sealed class ChatClient
{
    private readonly MistralClient _client;

    internal ChatClient(MistralClient client)
    {
        _client = client;
    }

    public Task<ChatCompletionResponse> CompleteAsync(ChatCompletionRequest request, CancellationToken cancellationToken = default)
        => _client.SendJsonAsync<ChatCompletionRequest, ChatCompletionResponse>(HttpMethod.Post, "/v1/chat/completions", request, cancellationToken);

    public async IAsyncEnumerable<string> StreamAsync(
        ChatCompletionRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (string line in _client.StreamSseAsync(HttpMethod.Post, "/v1/chat/completions", request, cancellationToken).ConfigureAwait(false))
        {
            yield return line;
        }
    }
}
