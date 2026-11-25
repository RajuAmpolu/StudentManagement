using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client.Extensions.Msal;
using StudentManagement.Models;

namespace StudentManagement.Services
{
    public class AzureBlobStorageService : IFileStorageService
    {
        private readonly BlobContainerClient _containerClient;

        public AzureBlobStorageService(
            IConfiguration configuration)
        {
            string connectionString =
                configuration["BlobStorage:ConnectionString"]
                ?? throw new InvalidOperationException(
                    "Azure Blob connection string is missing.");

            string containerName =
                configuration["BlobStorage:ContainerName"]
                ?? throw new InvalidOperationException(
                    "Azure Blob container name is missing.");

            BlobServiceClient blobServiceClient =
                new BlobServiceClient(connectionString);

            _containerClient =
                blobServiceClient.GetBlobContainerClient(
                    containerName);

            _containerClient.CreateIfNotExists();
        }

        public async Task<(string StoredFileName,
            string FilePath)>
            UploadFileAsync(IFormFile file)
        {
            string extension =
                Path.GetExtension(file.FileName);

            string storedFileName =
                $"{Guid.NewGuid()}{extension}";

            BlobClient blobClient =
                _containerClient.GetBlobClient(
                    storedFileName);

            using Stream stream =
                file.OpenReadStream();

            await blobClient.UploadAsync(
                stream,
                new BlobUploadOptions
                {
                    HttpHeaders =
                        new BlobHttpHeaders
                        {
                            ContentType =
                                file.ContentType
                        }
                });

            return (
                storedFileName,
                blobClient.Uri.ToString());
        }

        public async Task<(Stream Stream,
            string ContentType,
            string FileName)?>
            DownloadFileAsync(
                string storedFileName)
        {
            BlobClient blobClient =
                _containerClient.GetBlobClient(
                    storedFileName);

            if (!await blobClient.ExistsAsync())
            {
                return null;
            }

            BlobDownloadInfo result =
                await blobClient.DownloadAsync();

            return (
                result.Content,
                result.ContentType ??
                    "application/octet-stream",
                storedFileName);
        }

        public async Task<bool>
            DeleteFileAsync(
                string storedFileName)
        {
            BlobClient blobClient =
                _containerClient.GetBlobClient(
                    storedFileName);

            var response =
                await blobClient.DeleteIfExistsAsync();

            return response.Value;
        }
    }
}
