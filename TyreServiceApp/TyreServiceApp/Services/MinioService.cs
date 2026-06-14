using Minio;
using Minio.DataModel.Args;

namespace TyreServiceApp.Services;

public class MinioService : IMinioService
{
    private readonly IMinioClient? _minio;
    private readonly string _bucket;
    private readonly string _endpoint;
    private readonly bool _enabled;
    private readonly bool _useSsl;

    public MinioService(IConfiguration configuration)
    {
        _endpoint = configuration["Minio:Endpoint"] ?? string.Empty;
        var accessKey = configuration["Minio:AccessKey"] ?? string.Empty;
        var secretKey = configuration["Minio:SecretKey"] ?? string.Empty;
        _bucket = configuration["Minio:Bucket"] ?? string.Empty;
        _useSsl = configuration.GetValue<bool?>("Minio:UseSSL") ?? false;
        var region = configuration["Minio:Region"] ?? string.Empty;
        _enabled = !string.IsNullOrWhiteSpace(_endpoint)
            && !string.IsNullOrWhiteSpace(accessKey)
            && !string.IsNullOrWhiteSpace(secretKey)
            && !string.IsNullOrWhiteSpace(_bucket);

        if (_enabled)
        {
            var client = new MinioClient()
                .WithEndpoint(_endpoint)
                .WithCredentials(accessKey, secretKey)
                .WithSSL(_useSsl);

            if (!string.IsNullOrWhiteSpace(region))
                client = client.WithRegion(region);

            _minio = client.Build();
        }
    }

    public async Task<string> UploadAsync(IFormFile file, int clientId, string prefix = "cars")
    {
        if (!_enabled || _minio == null || file.Length == 0)
            return string.Empty;

        var ext = Path.GetExtension(file.FileName);
        var objectName = $"{prefix}/{clientId}/{Guid.NewGuid()}{ext}";

        await using var stream = file.OpenReadStream();

        var args = new PutObjectArgs()
            .WithBucket(_bucket)
            .WithObject(objectName)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length)
            .WithContentType(file.ContentType);

        await _minio.PutObjectAsync(args);

        return objectName;
    }

    public async Task DeleteAsync(string objectName)
    {
        if (!_enabled || _minio == null || string.IsNullOrEmpty(objectName)) return;

        var args = new RemoveObjectArgs()
            .WithBucket(_bucket)
            .WithObject(objectName);

        await _minio.RemoveObjectAsync(args);
    }

    public async Task<string> GetFileUrlAsync(string objectName)
    {
        if (!_enabled || _minio == null || string.IsNullOrEmpty(objectName)) return "";

        try
        {
            var args = new PresignedGetObjectArgs()
                .WithBucket(_bucket)
                .WithObject(objectName)
                .WithExpiry(60 * 60 * 24 * 7);

            return await _minio.PresignedGetObjectAsync(args);
        }
        catch
        {
            var scheme = _useSsl ? "https" : "http";
            return $"{scheme}://{_endpoint}/{_bucket}/{objectName}";
        }
    }
}
