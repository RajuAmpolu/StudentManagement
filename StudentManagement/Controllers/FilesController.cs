using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StudentManagement.Models;
using StudentManagement.Repositories;
using StudentManagement.Services;

namespace StudentManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IFileStorageService _storage;
        private readonly FileRepository _repository;
        private IConfiguration _config;

        public FilesController(
            IFileStorageService storage,
            FileRepository repository,
            IConfiguration config)
        {
            _storage = storage;
            _repository = repository;
            _config = config;
        }


        // ==========================================
        // UPLOAD
        // ==========================================

        [HttpPost("upload")]
        public async Task<IActionResult> Upload(
            IFormFile file)
        {
            string StoredFileName = "";
            try
            {
                if (file == null ||
                    file.Length == 0)
                {
                    return BadRequest(
                        "Please select a file.");
                }

                if (file.Length >
                    10 * 1024 * 1024)
                {
                    return BadRequest(
                        "Maximum file size is 10 MB.");
                }

                // Upload physical file
                // Local folder OR Azure Blob

                var storageResult =
                     await _storage.UploadFileAsync(file);
                StoredFileName = storageResult.StoredFileName;
                // Save metadata in SQL Server
                FileRecord record =
                    new FileRecord
                    {
                        OriginalFileName =
                            file.FileName,

                        StoredFileName =
                            storageResult.StoredFileName,

                        FilePath =
                            storageResult.FilePath,

                        FileSize =
                            file.Length,

                        ContentType =
                            file.ContentType,

                        CreatedDate =
                            DateTime.UtcNow
                    };

                int id = await _repository.InsertFile(record);

                return Ok(new
                {
                    message =
                        "File uploaded successfully",

                    id = id,

                    fileName =
                        record.OriginalFileName,

                    fileSize =
                        record.FileSize,

                    filePath =
                        record.FilePath,

                    createdDate =
                        record.CreatedDate
                });
            }
            catch (Exception ex)
            {
                // 3. If SQL fails, delete uploaded Blob
                if (!string.IsNullOrEmpty(StoredFileName))
                {
                    try
                    {
                        await _storage.DeleteFileAsync(StoredFileName);
                    }
                    catch (Exception cleanupException)
                    {
                        // Log this failure for manual/background cleanup
                        Console.WriteLine(cleanupException);
                    }
                }

                return StatusCode(500, new
                {
                    message = "Upload failed. Blob cleanup was attempted.",
                    error = ex.Message
                });
            }
        }


        // ==========================================
        // DOWNLOAD
        // ==========================================

        [HttpGet("download/{id}")]
        public async Task<IActionResult> Download(
            int id)
        {
            // Get metadata from SQL
            FileRecord? file =
                await _repository.GetFileById(id);

            if (file == null)
            {
                return NotFound(
                    "File record not found.");
            }

            // Get actual file from
            // Local Storage OR Azure Blob
            var result =
                await _storage
                    .DownloadFileAsync(
                        file.StoredFileName);

            if (result == null)
            {
                return NotFound(
                    "Physical file not found.");
            }

            // THIS IS WHERE return File() GOES
            return File(
                result.Value.Stream,
                result.Value.ContentType,
                file.OriginalFileName);
        }


        // ==========================================
        // DELETE
        // ==========================================

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(
            int id)
        {
            // Get metadata from SQL
            FileRecord? file =
                await _repository
                    .GetFileById(id);

            if (file == null)
            {
                return NotFound(
                    "File record not found.");
            }

            // Delete physical file
            // from Local Storage OR Azure Blob
            bool deleted =
                await _storage
                    .DeleteFileAsync(
                        file.StoredFileName);

            if (!deleted)
            {
                return NotFound(
                    "Physical file not found.");
            }

            // Delete metadata from SQL
            await _repository
                .DeleteFile(id);

            return Ok(new
            {
                message =
                    "File deleted successfully"
            });
        }

        [HttpGet("GetAllFiles")]
        public async Task<IActionResult> GetAllFiles()
        {
            var files = await _repository.GetAllFiles();

            return Ok(files);
        }

        [HttpGet("GetFile/{id}")]
        public async Task<IActionResult> GetFile(int id)
        {
            var file = await _repository.GetFileById(id);

            if (file == null)
            {
                return NotFound(new
                {
                    message = "File not found."
                });
            }

            var result =
                await _storage.DownloadFileAsync(
                    file.StoredFileName);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "Physical file not found."
                });
            }

            return File(
                result.Value.Stream,
                result.Value.ContentType,
                file.OriginalFileName);
        }
    }
}
