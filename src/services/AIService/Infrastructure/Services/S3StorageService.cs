using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AIService.Infrastructure.Services;

public interface IS3StorageService
{
    Task<string> UploadFileAsync(Stream stream, string key, string contentType);
    Task<string> GetPresignedUrlAsync(string key, TimeSpan expiry);
}

public class S3StorageService : IS3StorageService
{
    private readonly IAmazonS3 _s3;
    private readonly string _bucketName;
    private readonly ILogger<S3StorageService> _logger;

    public S3StorageService(IAmazonS3 s3, IConfiguration configuration, ILogger<S3StorageService> logger)
    {
        _s3 = s3;
        _bucketName = configuration["AWS_S3_BUCKET"] ?? "matchura-uploads";
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(Stream stream, string key, string contentType)
    {
        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = stream,
            ContentType = contentType,
        };

        await _s3.PutObjectAsync(request);
        _logger.LogInformation("Uploaded file to S3: {Key}", key);
        return key;
    }

    public async Task<string> GetPresignedUrlAsync(string key, TimeSpan expiry)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = key,
            Expires = DateTime.UtcNow.Add(expiry),
        };

        var url = await _s3.GetPreSignedURLAsync(request);
        return url;
    }
}
