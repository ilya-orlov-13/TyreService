using System.Net.Http.Headers;
using System.Text.Json;
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
    private readonly string _supabaseUrl;
    private readonly string? _serviceRoleKey;
    private readonly ILogger<MinioService> _logger;
    private readonly HttpClient _http;
    private readonly bool _useRestApi;

    public MinioService(IConfiguration configuration, ILogger<MinioService> logger, IHttpClientFactory httpFactory)
    {
        _logger = logger;
        _serviceUrl = configuration["Minio:ServiceURL"] ?? string.Empty;
        var accessKey = configuration["Minio:AccessKey"] ?? string.Empty;
        var secretKey = configuration["Minio:SecretKey"] ?? string.Empty;
        _bucket = configuration["Minio:Bucket"] ?? string.Empty;
        var region = configuration["Minio:Region"] ?? string.Empty;

        _supabaseUrl = configuration["Supabase:Url"] ?? configuration["Minio:SupabaseUrl"] ?? string.Empty;
        _serviceRoleKey = configuration["Supabase:ServiceRoleKey"] ?? configuration["Minio:ServiceRoleKey"];

        _useRestApi = !string.IsNullOrWhiteSpace(_supabaseUrl) && !string.IsNullOrWhiteSpace(_serviceRoleKey);

        if (_useRestApi)
        {
            _http = httpFactory?.CreateClient() ?? new HttpClient();
            _http.BaseAddress = new Uri(_supabaseUrl.TrimEnd('/'));
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _serviceRoleKey);
            _http.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            _s3 = null;
            _enabled = true;

            _logger.LogInformation(
                "Minio REST API initialised: SupabaseUrl={Url}, Bucket={Bucket}",
                _supabaseUrl, _bucket);
            return;
        }

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
                    AuthenticationRegion = string.IsNullOrWhiteSpace(region) ? "us-east-1" : region
                };

                var credentials = new BasicAWSCredentials(accessKey, secretKey);
                _s3 = new AmazonS3Client(credentials, config);

                _http = httpFactory?.CreateClient() ?? new HttpClient();

                _logger.LogInformation(
                    "S3 client initialised: ServiceURL={ServiceUrl}, Bucket={Bucket}, Region={Region}",
                    _serviceUrl, _bucket, region);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialise S3 client");
                _s3 = null;
                _http = httpFactory?.CreateClient() ?? new HttpClient();
            }
        }
        else
        {
            _http = httpFactory?.CreateClient() ?? new HttpClient();
            _logger.LogWarning(
                "Minio disabled — missing configuration (ServiceURL={Url}, AccessKey={Ak}, SecretKey={Sk}, Bucket={B})",
                !string.IsNullOrWhiteSpace(_serviceUrl),
                !string.IsNullOrWhiteSpace(accessKey),
                !string.IsNullOrWhiteSpace(secretKey),
                !string.IsNullOrWhiteSpace(_bucket));
        }
    }

    public async Task<string> UploadAsync(IFormFile file, int clientId, string prefix = "cars")
    {
        if (!_enabled)
        {
            _logger.LogWarning("Upload skipped — Minio disabled");
            return string.Empty;
        }

        if (file.Length == 0)
        {
            _logger.LogWarning("Upload skipped — empty file");
            return string.Empty;
        }

        var ext = Path.GetExtension(file.FileName);
        var key = $"{prefix}/{clientId}/{Guid.NewGuid()}{ext}";

        if (_useRestApi)
        {
            return await UploadViaRestApiAsync(file, key);
        }

        return await UploadViaS3SdkAsync(file, key);
    }

    private async Task<string> UploadViaRestApiAsync(IFormFile file, string key)
    {
        try
        {
            await using var stream = file.OpenReadStream();
            var content = new StreamContent(stream);
            content.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");

            var url = $"/storage/v1/object/{_bucket}/{key}";
            var response = await _http.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogError("REST upload failed: {Status}, Body={Body}", response.StatusCode, body);
                throw new HttpRequestException($"Supabase REST API returned {response.StatusCode}: {body}");
            }

            _logger.LogInformation("REST upload succeeded: Key={Key}, Size={Size}", key, file.Length);
            return key;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "REST upload failed: Key={Key}", key);
            throw;
        }
    }

    private async Task<string> UploadViaS3SdkAsync(IFormFile file, string key)
    {
        if (_s3 == null)
        {
            _logger.LogWarning("S3 SDK upload skipped — client not initialised");
            return string.Empty;
        }

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
        if (!_enabled || string.IsNullOrEmpty(objectName))
        {
            _logger.LogWarning("Delete skipped — Minio disabled or empty key");
            return;
        }

        if (_useRestApi)
        {
            await DeleteViaRestApiAsync(objectName);
            return;
        }

        await DeleteViaS3SdkAsync(objectName);
    }

    private async Task DeleteViaRestApiAsync(string objectName)
    {
        try
        {
            var url = $"/storage/v1/object/{_bucket}/{objectName}";
            var response = await _http.DeleteAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("REST delete returned {Status}: {Body}", response.StatusCode, body);
                return;
            }

            _logger.LogInformation("REST delete succeeded: Key={Key}", objectName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "REST delete failed: Key={Key}", objectName);
        }
    }

    private async Task DeleteViaS3SdkAsync(string objectName)
    {
        if (_s3 == null) return;

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
        if (!_enabled || string.IsNullOrEmpty(objectName))
            return Task.FromResult(string.Empty);

        if (_useRestApi)
        {
            var baseUrl = _supabaseUrl.TrimEnd('/');
            return Task.FromResult($"{baseUrl}/storage/v1/object/public/{_bucket}/{objectName}");
        }

        if (_s3 == null)
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
        if (!_enabled)
            return false;

        if (_useRestApi)
        {
            try
            {
                var response = await _http.GetAsync("/storage/v1/bucket");
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("REST health check passed");
                    return true;
                }

                var body = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("REST health check failed: {Status}, Body={Body}", response.StatusCode, body);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "REST health check failed");
                return false;
            }
        }

        if (_s3 == null)
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
