using MistralSdk.Models;

namespace MistralSdk.Clients;

public sealed class FilesClient
{
    private readonly MistralClient _client;

    internal FilesClient(MistralClient client)
    {
        _client = client;
    }

    public Task<FileUploadResponse> UploadAsync(Stream file, string fileName, string? purpose = null, CancellationToken cancellationToken = default)
        => _client.UploadFileAsync(file, fileName, purpose, cancellationToken);
}
