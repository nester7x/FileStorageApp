using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Logging;
using Moq;

namespace BlobTriggerFunction.Tests;

[TestFixture]
public class BlobTriggerFunctionTests
{
    [Test]
    public async Task BlobTrigger_WithValidEmailMetadata_LogStartProcessing()
    {
        // Arrange
        var fakeLogger = new FakeLogger();
        var fakeBlob = new FakeCloudBlockBlob(new Uri("https://your.blob.core.windows.net/documents/test.txt"));
        fakeBlob.Metadata.Add("email", "recipient@example.com");

        // Act
        await BlobTrigger.Run(fakeBlob, "test.txt", fakeLogger);

        // Assert
        Assert.IsTrue(fakeLogger.Logs.Contains("Blob trigger function Processed blob\n Name:test.txt"));
    }

    [Test]
    public async Task BlobTrigger_WithoutEmailMetadata_LogWarning()
    {
        // Arrange
        var fakeLogger = new FakeLogger();
        var fakeBlob = new FakeCloudBlockBlob(new Uri("https://your.blob.core.windows.net/documents/test.txt"));

        // Act
        await BlobTrigger.Run(fakeBlob, "test.txt", fakeLogger);

        // Assert
        Assert.IsTrue(fakeLogger.Logs.Contains("Email metadata not found in blob metadata."));
    }
}