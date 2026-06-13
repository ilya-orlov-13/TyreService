using Minio;
using Minio.DataModel.Args;

namespace TyreServiceApp.Services;

public class MinioService : IMinioService
{
    private readonly IMinioClient _minio;
    private readonly string _bucket;
    private readonly string _endpoint;

    public MinioService(IConfiguration configuration)
    {
        _endpoint = configuration["Minio:Endpoint"]!;
        var accessKey = configuration["Minio:AccessKey"]!;
        var secretKey = configuration["Minio:SecretKey"]!;
        _bucket = configuration["Minio:Bucket"]!;

        _minio = new MinioClient()
            .WithEndpoint(_endpoint)
            .WithCredentials(accessKey, secretKey)
            .WithSSL(false)
            .Build();
    }

    public async Task<string> UploadAsync(IFormFile file, int clientId, string prefix = "cars")
    {
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
        if (string.IsNullOrEmpty(objectName)) return;

        var args = new RemoveObjectArgs()
            .WithBucket(_bucket)
            .WithObject(objectName);

        await _minio.RemoveObjectAsync(args);
    }

    public async Task<string> GetFileUrlAsync(string objectName)
    {
        if (string.IsNullOrEmpty(objectName)) return "";

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
            return $"http://{_endpoint}/{_bucket}/{objectName}";
        }
    }
}
