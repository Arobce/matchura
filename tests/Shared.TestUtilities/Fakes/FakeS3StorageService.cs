using AIService.Infrastructure.Services;

namespace Shared.TestUtilities.Fakes;

public class FakeS3StorageService : IS3StorageService
{
    public Dictionary<string, byte[]> Files { get; } = new();

    public async Task<string> UploadFileAsync(Stream stream, string key, string contentType)
    {
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        Files[key] = ms.ToArray();
        return $"https://fake-s3.local/{key}";
    }

    public Task<string> GetPresignedUrlAsync(string key, TimeSpan expiry)
    {
        return Task.FromResult($"https://fake-s3.local/{key}?expires={expiry.TotalSeconds}");
    }
}
