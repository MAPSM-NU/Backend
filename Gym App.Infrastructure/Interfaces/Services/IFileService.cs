
using Gym_App.Infastructure.Transfer_Classes;
using Microsoft.AspNetCore.Http;

namespace Gym_App.Infrastructure.Interfaces.Services
{
    public interface IFileService
    {
        public Task<SettersResponse> UploadFileAsync(IFormFile image, string[] allowedFileExtensions);
        public Task DeleteFileAsync(string imageUrl);
    }
}
