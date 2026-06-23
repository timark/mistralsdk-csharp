using System.Net;
using System.Text;
using System.Text.Json;
using MistralSdk;
using MistralSdk.Models;

namespace MistralSdk.Tests;

public sealed class MistralClientTests
{
    [Fact]
    public async Task CompleteAsync_SendsBearerTokenAndBody()
    {
        var handler = new RecordingHandler(_ =>
        {
            var payload = JsonSerializer.Serialize(new ChatCompletionResponse
            {
                Id = "chatcmpl-1",
                Model = "mistral-large-latest",
                Choices =
                [
                    new ChatChoice
                    {
                        Index = 0,
                        Message = new ChatMessage { Role = "assistant", Content = "hello" },
                    },
                ],
            });
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json"),
            };
        });

        using var client = new MistralClient(new MistralClientOptions
        {
            ApiKey = "test-key",
            Transport = handler,
        });

        ChatCompletionResponse response = await client.Chat.CompleteAsync(new ChatCompletionRequest
        {
            Model = "mistral-large-latest",
            Messages = [new ChatMessage { Role = "user", Content = "hi" }],
        });

        Assert.Equal("Bearer", handler.LastRequest?.Headers.Authorization?.Scheme);
        Assert.Equal("test-key", handler.LastRequest?.Headers.Authorization?.Parameter);
        Assert.Equal("chatcmpl-1", response.Id);
    }

    [Fact]
    public async Task CompleteAsync_RetriesOnServerError()
    {
        var attempt = 0;
        var handler = new RecordingHandler(_ =>
        {
            attempt++;
            if (attempt == 1)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent("{}", Encoding.UTF8, "application/json"),
                };
            }

            var payload = JsonSerializer.Serialize(new ChatCompletionResponse
            {
                Id = "chatcmpl-ok",
            });
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json"),
            };
        });

        using var client = new MistralClient(new MistralClientOptions
        {
            ApiKey = "test-key",
            Transport = handler,
            RetryBaseDelay = TimeSpan.FromMilliseconds(1),
            MaxRetryAttempts = 1,
        });

        ChatCompletionResponse response = await client.Chat.CompleteAsync(new ChatCompletionRequest
        {
            Model = "mistral-large-latest",
            Messages = [new ChatMessage { Role = "user", Content = "hi" }],
        });

        Assert.Equal("chatcmpl-ok", response.Id);
        Assert.Equal(2, attempt);
    }

    private sealed class RecordingHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responseFactory;

        public RecordingHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
        {
            _responseFactory = responseFactory;
        }

        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(_responseFactory(request));
        }
    }
}
