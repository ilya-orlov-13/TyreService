namespace TyreServiceApp.Services;

public interface IMinioService
{
    Task<string> UploadAsync(IFormFile file, int clientId, string prefix = "cars");
    Task DeleteAsync(string objectName);
    Task<string> GetFileUrlAsync(string objectName);
}
