using System.Net;

namespace MistralSdk;

public sealed class MistralApiException : Exception
{
    public MistralApiException(HttpStatusCode statusCode, string? responseBody)
        : base($"Mistral API request failed with status {(int)statusCode} ({statusCode}).")
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }

    public HttpStatusCode StatusCode { get; }

    public string? ResponseBody { get; }
}
