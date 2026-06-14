using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.Runtime;

namespace TyreServiceApp.Services;

public class MinioService : IMinioService
{
    private readonly IAmazonS3? _s3;
    private readonly string _bucket;
    private readonly bool _enabled;
    private readonly string _serviceUrl;

    public MinioService(IConfiguration configuration)
    {
        _serviceUrl = configuration["Minio:ServiceURL"] ?? string.Empty;
        var accessKey = configuration["Minio:AccessKey"] ?? string.Empty;
        var secretKey = configuration["Minio:SecretKey"] ?? string.Empty;
        _bucket = configuration["Minio:Bucket"] ?? string.Empty;
        var region = configuration["Minio:Region"] ?? string.Empty;

        _enabled = !string.IsNullOrWhiteSpace(_serviceUrl)
            && !string.IsNullOrWhiteSpace(accessKey)
            && !string.IsNullOrWhiteSpace(secretKey)
            && !string.IsNullOrWhiteSpace(_bucket);

        if (_enabled)
        {
            var config = new AmazonS3Config
            {
                ServiceURL = _serviceUrl,
                ForcePathStyle = true,
                AuthenticationRegion = region
            };

            var credentials = new BasicAWSCredentials(accessKey, secretKey);
            _s3 = new AmazonS3Client(credentials, config);
        }
    }

    public async Task<string> UploadAsync(IFormFile file, int clientId, string prefix = "cars")
    {
        if (!_enabled || _s3 == null || file.Length == 0)
            return string.Empty;

        var ext = Path.GetExtension(file.FileName);
        var key = $"{prefix}/{clientId}/{Guid.NewGuid()}{ext}";

        await using var stream = file.OpenReadStream();

        var request = new PutObjectRequest
        {
            BucketName = _bucket,
            Key = key,
            InputStream = stream,
            ContentType = file.ContentType ?? "application/octet-stream",
            AutoCloseStream = true
        };

        await _s3.PutObjectAsync(request);
        return key;
    }

    public async Task DeleteAsync(string objectName)
    {
        if (!_enabled || _s3 == null || string.IsNullOrEmpty(objectName)) return;

        var request = new DeleteObjectRequest
        {
            BucketName = _bucket,
            Key = objectName
        };

        await _s3.DeleteObjectAsync(request);
    }

    public Task<string> GetFileUrlAsync(string objectName)
    {
        if (!_enabled || _s3 == null || string.IsNullOrEmpty(objectName))
            return Task.FromResult(string.Empty);

        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _bucket,
                Key = objectName,
                Expires = DateTime.UtcNow.AddDays(7),
                Protocol = Protocol.HTTPS
            };

            var url = _s3.GetPreSignedURL(request);
            return Task.FromResult(url);
        }
        catch
        {
            var baseUrl = _serviceUrl.TrimEnd('/');
            return Task.FromResult($"{baseUrl}/{_bucket}/{objectName}");
        }
    }
}
