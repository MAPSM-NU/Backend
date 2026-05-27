using Gym_App.Infastructure.Transfer_Classes;
using Gym_App.Infrastructure.Interfaces.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Gym_App.Application.Services
{
    public class FileService : IFileService
    {
        private readonly ILogger<FileService> _logger;
        private readonly IWebHostEnvironment environment;
        public FileService(ILogger<FileService> logger, IWebHostEnvironment environment)
        {
            _logger = logger;
            this.environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }
        public async Task<SettersResponse> UploadFileAsync(IFormFile image, string[] allowedFileExtensions)
        {
            try
            {
                if (image == null || image.Length == 0)
                {
                    _logger.LogWarning("There was an attempt to upload an empty file.");
                    return new SettersResponse { status = 0, msg = "No file uploaded." };
                }


                var contentPath = environment.ContentRootPath;
                var path = Path.Combine(contentPath,"wwwroot", "Uploads");

                if (!Directory.Exists(path))
                {
                    _logger.LogInformation($"Uploads directory does not exist. Creating directory at {path}.");
                    Directory.CreateDirectory(path);
                }


                var ext = Path.GetExtension(image.FileName);
                if (!allowedFileExtensions.Contains(ext))
                {
                    _logger.LogWarning($"File type '{ext}' is not allowed for upload.");
                    _logger.LogInformation($"Allowed file types are: {string.Join(", ", allowedFileExtensions)}.");
                    return new SettersResponse { status = 0, msg = "File type is not allowed." };
                }

                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(path, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }
                _logger.LogInformation($"Successful creation of file '{fileName}' at '{filePath}'.");
                return new SettersResponse { status = 2, msg = fileName};
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while uploading a file.");
                return new SettersResponse { status = 0, msg = ex.Message };
            }

        }
        public Task DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl))
            {
                _logger.LogWarning("some dumb ahh attempted to delete a file with a empty URL smh");
                throw new ArgumentException("File URL is null or empty.", nameof(fileUrl));
            }

            var contentPath = environment.ContentRootPath;
            var path = Path.Combine(contentPath, "wwwroot", "Uploads", fileUrl);

            if (!File.Exists(path))
            {
                _logger.LogWarning($"Attempted deletion of file at '{path}', but the file was not found.");
                throw new FileNotFoundException("File not found.", path);
            }

            File.Delete(path);
            _logger.LogInformation($"Successful deletion of file at '{path}'.");
            return Task.CompletedTask;
        }
    }
}
