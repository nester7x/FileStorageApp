using System;
using System.Threading.Tasks;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using MailKit.Net.Smtp;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace BlobTriggerFunction;

public class BlobTrigger
{
    private readonly string StorageConnectionString = Environment.GetEnvironmentVariable("ACCESS_KEY");
    private readonly string GmailPassword = Environment.GetEnvironmentVariable("GOOGLE_PASS");
    private readonly string MyEmail = Environment.GetEnvironmentVariable("MY_EMAIL");

    private readonly ILogger _logger;

    [FunctionName("BlobTrigger")]
    public static async Task Run([BlobTrigger("documents/{name}")]CloudBlockBlob myBlob, string name, ILogger log)
    {
        log.LogInformation($"Blob trigger function Processed blob\n Name:{name}");

        var isExist = myBlob.Metadata.TryGetValue("email", out string email);

        if (!isExist)
        {
            log.LogWarning("Email metadata not found in blob metadata.");
            return;
        }

        var sasToken = GenerateSasToken(myBlob.Uri);
        await SendEmailWithSasToken(email, myBlob.Uri, sasToken, log);
    }

    private static string GenerateSasToken(Uri blobUri)
    {
        var blobClient = new BlobClient(blobUri, new BlobClientOptions());
        var sasBuilder = new BlobSasBuilder()
        {
            BlobContainerName = blobClient.BlobContainerName,
            BlobName = blobClient.Name,
            Resource = "b",
            StartsOn = DateTimeOffset.UtcNow,
            ExpiresOn = DateTimeOffset.UtcNow.AddHours(1),
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        var sasToken = sasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential(blobClient.AccountName, StorageConnectionString)).ToString();
        return sasToken;
    }

    private static async Task SendEmailWithSasToken(string email, Uri blobUri, string sasToken, ILogger log)
    {
        var blobUriWithSasToken = new UriBuilder(blobUri)
        {
            Query = sasToken
        }.Uri;

        var message = new MimeMessage();

        message.From.Add(new MailboxAddress("Andrii", MyEmail));
        message.To.Add(new MailboxAddress("Andrii", email));

        message.Subject = "Your file has been uploaded!";
        message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
        {
            Text = $"<b>Hi! Here's your <a href=\"{blobUriWithSasToken}\">file</a></b>"
        };

        using (var smtp = new SmtpClient())
        {
            smtp.Connect("smtp.gmail.com", 587, false);

            smtp.Authenticate(MyEmail, GmailPassword);

            smtp.Send(message);
            smtp.Disconnect(true);
        }

        log.LogInformation("Email sended!");

    }
}
