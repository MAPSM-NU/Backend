using Gym_App.Infastructure.Transfer_Classes;
using Microsoft.AspNetCore.Http;
using System.Threading;

namespace Gym_App.Infrastructure.Interfaces.Services
{
    public interface IFileService
    {
        public Task<SettersResponse> UploadFileAsync(IFormFile image, string[] allowedFileExtensions, CancellationToken cancellationToken = default);
        public Task DeleteFileAsync(string imageUrl, CancellationToken cancellationToken = default);
    }
}
