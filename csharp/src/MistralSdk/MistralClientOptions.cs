namespace MistralSdk;

public sealed class MistralClientOptions
{
    public string ApiKey { get; init; } = string.Empty;
    public Uri BaseUri { get; init; } = new("https://api.mistral.ai");
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(100);
    public int MaxRetryAttempts { get; init; } = 3;
    public TimeSpan RetryBaseDelay { get; init; } = TimeSpan.FromMilliseconds(250);
    public HttpMessageHandler? Transport { get; init; }
}
