using FileStorageApp.Controllers;
using FileStorageApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;

namespace FileStorageApp.Tests;

[TestFixture]
public class FileControllerTests
{
    private Mock<ILogger<FileController>> _loggerMock;
    private Mock<IFileService> _fileStorageServiceMock;
    private FileController _fileController;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<FileController>>();
        _fileStorageServiceMock = new Mock<IFileService>();
        _fileController = new FileController(_loggerMock.Object, _fileStorageServiceMock.Object);
    }

    [Test]
    public async Task UploadFile_ReturnsBadRequest_WhenRequestIsNull()
    {
        // Arrange
        SaveFileRequest request = null;

        // Act
        var result = await _fileController.UploadFile(request);

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
    }

    [Test]
    public async Task UploadFile_ReturnsBadRequest_WhenFormFileIsEmpty()
    {
        // Arrange
        var request = new SaveFileRequest { FormFile = new FormFile(null, 0, 0, "test", "test.txt") };

        // Act
        var result = await _fileController.UploadFile(request);

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
    }

    [Test]
    public async Task UploadFile_ReturnsInternalServerError_WhenExceptionThrown()
    {
        // Arrange
        var request = new SaveFileRequest
        {
            FormFile = new FormFile(new MemoryStream(Encoding.UTF8.GetBytes("Test data")), 0, 0, "test", "test.txt"),
            Email = "test@example.com"
        };

        _fileStorageServiceMock.Setup(x => x.UploadFile(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>())).ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _fileController.UploadFile(request);

        // Assert
        Assert.IsInstanceOf<ObjectResult>(result);
        var objectResult = (ObjectResult)result;
        Assert.AreEqual(400, objectResult.StatusCode);
    }
}
