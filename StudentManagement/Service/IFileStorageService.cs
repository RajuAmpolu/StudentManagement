namespace StudentManagement.Services
{

    public interface IFileStorageService
    {
        Task<(string StoredFileName, string FilePath)>
            UploadFileAsync(IFormFile file);

        Task<(Stream Stream, string ContentType, string FileName)?>
            DownloadFileAsync(string storedFileName);

        Task<bool>
            DeleteFileAsync(string storedFileName);
    }

}
