using System.Net;

namespace MistralSdk.Internal;

internal sealed class RetryHandler : DelegatingHandler
{
    private readonly int _maxRetryAttempts;
    private readonly TimeSpan _baseDelay;

    public RetryHandler(int maxRetryAttempts, TimeSpan baseDelay)
    {
        _maxRetryAttempts = Math.Max(0, maxRetryAttempts);
        _baseDelay = baseDelay;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        for (var attempt = 0; ; attempt++)
        {
            HttpRequestMessage requestCopy = await CloneRequestAsync(request, cancellationToken).ConfigureAwait(false);

            try
            {
                HttpResponseMessage response = await base.SendAsync(requestCopy, cancellationToken).ConfigureAwait(false);
                if (!ShouldRetry(response.StatusCode) || attempt >= _maxRetryAttempts)
                {
                    return response;
                }

                response.Dispose();
            }
            catch (HttpRequestException) when (attempt < _maxRetryAttempts)
            {
            }

            var delayMs = _baseDelay.TotalMilliseconds * Math.Pow(2, attempt);
            await Task.Delay(TimeSpan.FromMilliseconds(delayMs), cancellationToken).ConfigureAwait(false);
        }
    }

    private static bool ShouldRetry(HttpStatusCode statusCode)
        => statusCode == HttpStatusCode.RequestTimeout
           || statusCode == HttpStatusCode.TooManyRequests
           || (int)statusCode >= 500;

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);

        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        if (request.Content is not null)
        {
            var memory = new MemoryStream();
            await request.Content.CopyToAsync(memory, cancellationToken).ConfigureAwait(false);
            memory.Position = 0;
            var content = new StreamContent(memory);
            foreach (var header in request.Content.Headers)
            {
                content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            clone.Content = content;
        }

        return clone;
    }
}
