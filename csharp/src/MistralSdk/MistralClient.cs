using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using MistralSdk.Clients;
using MistralSdk.Internal;
using MistralSdk.Models;

namespace MistralSdk;

public sealed class MistralClient : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly HttpClient _httpClient;
    private readonly bool _ownsClient;

    public MistralClient(MistralClientOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ApiKey))
        {
            throw new ArgumentException("ApiKey is required.", nameof(options));
        }

        HttpMessageHandler handler = options.Transport ?? new HttpClientHandler();
        var retryHandler = new RetryHandler(options.MaxRetryAttempts, options.RetryBaseDelay)
        {
            InnerHandler = handler,
        };

        _httpClient = new HttpClient(retryHandler)
        {
            BaseAddress = options.BaseUri,
            Timeout = options.Timeout,
        };
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _ownsClient = true;

        Chat = new ChatClient(this);
        Embeddings = new EmbeddingsClient(this);
        Files = new FilesClient(this);
    }

    public ChatClient Chat { get; }

    public EmbeddingsClient Embeddings { get; }

    public FilesClient Files { get; }

    internal async Task<TResponse> SendJsonAsync<TRequest, TResponse>(HttpMethod method, string path, TRequest request, CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(method, path)
        {
            Content = new StringContent(JsonSerializer.Serialize(request, JsonOptions), Encoding.UTF8, "application/json"),
        };

        using HttpResponseMessage response = await _httpClient.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        return await DeserializeResponseAsync<TResponse>(response, cancellationToken).ConfigureAwait(false);
    }

    internal async Task<FileUploadResponse> UploadFileAsync(Stream file, string fileName, string? purpose, CancellationToken cancellationToken)
    {
        using var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(file);
        content.Add(fileContent, "file", fileName);
        if (!string.IsNullOrWhiteSpace(purpose))
        {
            content.Add(new StringContent(purpose), "purpose");
        }

        using var message = new HttpRequestMessage(HttpMethod.Post, "/v1/files")
        {
            Content = content,
        };

        using HttpResponseMessage response = await _httpClient.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        return await DeserializeResponseAsync<FileUploadResponse>(response, cancellationToken).ConfigureAwait(false);
    }

    internal async IAsyncEnumerable<string> StreamSseAsync<TRequest>(
        HttpMethod method,
        string path,
        TRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using var message = new HttpRequestMessage(method, path)
        {
            Content = new StringContent(JsonSerializer.Serialize(request, JsonOptions), Encoding.UTF8, "application/json"),
        };

        using HttpResponseMessage response = await _httpClient.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            string body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            throw new MistralApiException(response.StatusCode, body);
        }

        await using Stream stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var reader = new StreamReader(stream);
        while (true)
        {
            string? line = await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false);
            if (line is null)
            {
                yield break;
            }

            if (!line.StartsWith("data: ", StringComparison.Ordinal))
            {
                continue;
            }

            string payload = line[6..].Trim();
            if (payload == "[DONE]")
            {
                yield break;
            }

            yield return payload;
        }
    }

    private static async Task<TResponse> DeserializeResponseAsync<TResponse>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        string body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            throw new MistralApiException(response.StatusCode, body);
        }

        TResponse? parsed = JsonSerializer.Deserialize<TResponse>(body, JsonOptions);
        if (parsed is null)
        {
            throw new InvalidOperationException("Response body could not be deserialized.");
        }

        return parsed;
    }

    public void Dispose()
    {
        if (_ownsClient)
        {
            _httpClient.Dispose();
        }
    }
}
