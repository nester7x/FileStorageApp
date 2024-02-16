using FileStorageApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace FileStorageApp.Controllers;
[ApiController]
[Route("api/[controller]")]
public class FileController : ControllerBase
{
    private readonly ILogger<FileController> _logger;
    private readonly IFileService _fileStorageService;

    public FileController(ILogger<FileController> logger, IFileService fileService)
    {
        _logger = logger;
        _fileStorageService = fileService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile([FromForm] SaveFileRequest request)
    {
        if (request == null || request.FormFile.Length <= 0)
        {
            _logger.LogError("File is empty or null.");
            return BadRequest("File is empty or null.");
        }

        try
        {
            using (var memoryStream = new MemoryStream())
            {
                await request.FormFile.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                string fileName = Path.GetFileName(request.FormFile.FileName);
                string blobUri = await _fileStorageService.UploadFile(memoryStream, request.Email, fileName);

                _logger.LogInformation($"File '{fileName}' uploaded successfully to '{blobUri}'.");
                return Ok(blobUri);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"An error occurred while uploading file: {ex.Message}");
            return StatusCode(500, "An error occurred while uploading file.");
        }
    }
}
