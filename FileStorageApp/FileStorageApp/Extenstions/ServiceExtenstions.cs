using Azure.Storage.Blobs;
using FileStorageApp.Services;
namespace FileStorageApp.Extenstions;

public static class ServiceExtenstions
{
    public static void ConfigureServices(this IServiceCollection services)
    {
        services.AddCors(opt =>
        {
            opt.AddDefaultPolicy(builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        services.AddScoped<IFileService, AzureBlobStorageService>();
        services.AddScoped(provider =>
        {
            string connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");
            return new BlobServiceClient(connectionString);
        });
    }
}

