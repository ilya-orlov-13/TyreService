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
    private readonly ILogger<MinioService> _logger;

    public MinioService(IConfiguration configuration, ILogger<MinioService> logger)
    {
        _logger = logger;
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
            try
            {
                var config = new AmazonS3Config
                {
                    ServiceURL = _serviceUrl,
                    ForcePathStyle = true,
                    AuthenticationRegion = region
                };

                var credentials = new BasicAWSCredentials(accessKey, secretKey);
                _s3 = new AmazonS3Client(credentials, config);

                _logger.LogInformation(
                    "S3 client initialised: ServiceURL={ServiceUrl}, Bucket={Bucket}, Region={Region}",
                    _serviceUrl, _bucket, region);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialise S3 client");
                _s3 = null;
            }
        }
        else
        {
            _logger.LogWarning(
                "S3 disabled — missing configuration (ServiceURL={Url}, AccessKey={Ak}, SecretKey={Sk}, Bucket={B})",
                !string.IsNullOrWhiteSpace(_serviceUrl),
                !string.IsNullOrWhiteSpace(accessKey),
                !string.IsNullOrWhiteSpace(secretKey),
                !string.IsNullOrWhiteSpace(_bucket));
        }
    }

    public async Task<string> UploadAsync(IFormFile file, int clientId, string prefix = "cars")
    {
        if (!_enabled || _s3 == null)
        {
            _logger.LogWarning("S3 upload skipped — service disabled or not initialised");
            return string.Empty;
        }

        if (file.Length == 0)
        {
            _logger.LogWarning("S3 upload skipped — empty file");
            return string.Empty;
        }

        var ext = Path.GetExtension(file.FileName);
        var key = $"{prefix}/{clientId}/{Guid.NewGuid()}{ext}";

        try
        {
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
            _logger.LogInformation("S3 upload succeeded: Bucket={Bucket}, Key={Key}, Size={Size}",
                _bucket, key, file.Length);
            return key;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "S3 upload failed: Bucket={Bucket}, Key={Key}", _bucket, key);
            throw;
        }
    }

    public async Task DeleteAsync(string objectName)
    {
        if (!_enabled || _s3 == null || string.IsNullOrEmpty(objectName))
        {
            _logger.LogWarning("S3 delete skipped — service disabled or empty key");
            return;
        }

        try
        {
            var request = new DeleteObjectRequest
            {
                BucketName = _bucket,
                Key = objectName
            };

            await _s3.DeleteObjectAsync(request);
            _logger.LogInformation("S3 delete succeeded: Key={Key}", objectName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "S3 delete failed: Key={Key}", objectName);
        }
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "S3 pre-signed URL failed for Key={Key}, falling back to public URL", objectName);
            var baseUrl = _serviceUrl.TrimEnd('/');
            return Task.FromResult($"{baseUrl}/{_bucket}/{objectName}");
        }
    }

    public async Task<bool> HealthCheckAsync()
    {
        if (!_enabled || _s3 == null)
            return false;

        try
        {
            var response = await _s3.ListBucketsAsync();
            _logger.LogInformation("S3 health check passed — {Count} buckets available", response.Buckets.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "S3 health check failed");
            return false;
        }
    }
}
