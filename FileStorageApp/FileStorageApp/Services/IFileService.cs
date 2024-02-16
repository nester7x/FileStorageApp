namespace FileStorageApp.Services;

public interface IFileService
{
    Task<string> UploadFile(Stream stream, string email, string fileName);
}
