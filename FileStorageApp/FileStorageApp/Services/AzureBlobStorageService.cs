using Azure.Storage.Blobs;
using MailKit.Net.Smtp;
using MimeKit;

namespace FileStorageApp.Services;

public class AzureBlobStorageService : IFileService
{
    private readonly BlobServiceClient _blobServiceClient;

    public AzureBlobStorageService(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient ?? throw new ArgumentNullException(nameof(blobServiceClient));
    }

    public async Task<string> UploadFile(Stream stream, string email, string fileName)
    {
        string containerName = "documents";

        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        if (!await containerClient.ExistsAsync())
        {
            await containerClient.CreateAsync();
        }

        string blobName = $"{Guid.NewGuid()}-{fileName}";

        var blobClient = containerClient.GetBlobClient(blobName);
        await blobClient.UploadAsync(stream, true);

        blobClient.SetMetadata(new Dictionary<string, string>
        {
            { "email", email }
        });

        return blobClient.Uri.ToString();
    }
}