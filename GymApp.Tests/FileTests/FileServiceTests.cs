using Gym_App.Application.Services;
using Gym_App.Infastructure.Transfer_Classes;
using Gym_App.Infrastructure.Interfaces.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace GymApp.Tests.FileTests
{
    public class FileServiceTests : TestBase
    {
        private readonly Mock<ILogger<FileService>> _mockLogger;
        private readonly Mock<IWebHostEnvironment> _mockEnvironment;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<HttpContext> _mockHttpContext;
        private readonly Mock<HttpRequest> _mockHttpRequest;
        private readonly IFileService _fileService;
        private readonly string _testDirectory;

        public FileServiceTests() : base("FileTests")
        {
            // Initialize mocks
            _mockLogger = new Mock<ILogger<FileService>>();
            _mockEnvironment = new Mock<IWebHostEnvironment>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockHttpContext = new Mock<HttpContext>();
            _mockHttpRequest = new Mock<HttpRequest>();

            // Setup test directory
            _testDirectory = Path.Combine(Path.GetTempPath(), $"FileServiceTests-{Guid.NewGuid()}");
            Directory.CreateDirectory(_testDirectory);
            var wwwrootPath = Path.Combine(_testDirectory, "wwwroot", "Uploads");
            Directory.CreateDirectory(wwwrootPath);

            // Configure mocks
            _mockEnvironment.Setup(e => e.ContentRootPath).Returns(_testDirectory);
            _mockEnvironment.Setup(e => e.WebRootPath).Returns(Path.Combine(_testDirectory, "wwwroot"));

            // Setup HTTP context mocks
            _mockHttpRequest.Setup(r => r.Scheme).Returns("https");
            _mockHttpRequest.Setup(r => r.Host).Returns(new HostString("localhost:7210"));
            _mockHttpContext.Setup(c => c.Request).Returns(_mockHttpRequest.Object);
            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(_mockHttpContext.Object);

            // Create FileService instance
            _fileService = new FileService(_mockLogger.Object, _mockEnvironment.Object, _mockHttpContextAccessor.Object);
        }

        [Fact]
        public async Task UploadFilePngSucceed()
        {
            var fileName = "test-image.png";
            var fileContent = new byte[] { 137, 80, 78, 71 };
            var formFile = CreateMockFormFile(fileName, fileContent, "image/png");
            var allowedExtensions = new[] { ".png", ".jpg", ".jpeg" };

            var result = await _fileService.UploadFileAsync(formFile, allowedExtensions);

            Assert.NotNull(result);
            Assert.Equal(2, result.status);
            Assert.NotNull(result.msg);
            Assert.Contains("https://localhost:7210/Uploads/", result.msg);
            Assert.EndsWith(".png", result.msg);
        }

        [Fact]
        public async Task UploadFileJpgSucceed()
        {
            var fileName = "profile.jpg";
            var fileContent = new byte[] { 255, 216, 255, 224 };
            var formFile = CreateMockFormFile(fileName, fileContent, "image/jpeg");
            var allowedExtensions = new[] { ".png", ".jpg", ".jpeg" };

            var result = await _fileService.UploadFileAsync(formFile, allowedExtensions);

            Assert.Equal(2, result.status);
            Assert.NotEmpty(result.msg);
            Assert.Contains("/Uploads/", result.msg);
        }

        [Fact]
        public async Task UploadFileCreateDir()
        {
            var uploadsPath = Path.Combine(_testDirectory, "wwwroot", "Uploads");
            if (Directory.Exists(uploadsPath))
                Directory.Delete(uploadsPath, true);

            var fileName = "test.png";
            var fileContent = new byte[] { 137, 80, 78, 71 };
            var formFile = CreateMockFormFile(fileName, fileContent, "image/png");
            var allowedExtensions = new[] { ".png" };

            // ACT
            var result = await _fileService.UploadFileAsync(formFile, allowedExtensions);

            // ASSERT
            Assert.Equal(2, result.status);
            Assert.True(Directory.Exists(uploadsPath), "Uploads directory should have been created");
        }
        [Fact]
        public async Task UploadFileEmptyFile()
        {
            var emptyFile = CreateMockFormFile("empty.png", Array.Empty<byte>(), "image/png");
            var allowedExtensions = new[] { ".png" };

            var result = await _fileService.UploadFileAsync(emptyFile, allowedExtensions);

            Assert.Equal(0, result.status);
            Assert.Equal("No file uploaded.", result.msg);
        }
        [Fact]
        public async Task UploadFileNullFile()
        {
            var allowedExtensions = new[] { ".png" };

            var result = await _fileService.UploadFileAsync(null, allowedExtensions);

            Assert.Equal(0, result.status);
            Assert.Equal("No file uploaded.", result.msg);
        }
        [Fact]
        public async Task UploadFileWrongExtension()
        {
            var fileName = "bad-extensions-babyy.exe";
            var fileContent = new byte[] { 77, 90 };
            var formFile = CreateMockFormFile(fileName, fileContent, "application/octet-stream");
            var allowedExtensions = new[] { ".png", ".jpg", ".jpeg" };

            var result = await _fileService.UploadFileAsync(formFile, allowedExtensions);

            Assert.Equal(0, result.status);
            Assert.Contains("not allowed", result.msg);
        }
        [Fact]
        public async Task DeleteFileSucceed()
        {
            var fileName = "to-delete.png";
            var fileContent = new byte[] { 137, 80, 78, 71 };
            var formFile = CreateMockFormFile(fileName, fileContent, "image/png");
            var allowedExtensions = new[] { ".png" };

            var uploadResult = await _fileService.UploadFileAsync(formFile, allowedExtensions);
            var fileUrl = uploadResult.msg;

            var uploadsPath = Path.Combine(_testDirectory, "wwwroot", "Uploads");
            var filesBeforeDeletion = Directory.GetFiles(uploadsPath);
            Assert.NotEmpty(filesBeforeDeletion);

            var uploadedFileName = Path.GetFileName(fileUrl);

            await _fileService.DeleteFileAsync(uploadedFileName);

            var filesAfterDeletion = Directory.GetFiles(uploadsPath);
            Assert.Empty(filesAfterDeletion);
        }
        [Fact]
        public async Task DeleteFileNotFoundFile()
        {
            var nonExistentFileName = $"{Guid.NewGuid()}.png";

            await Assert.ThrowsAsync<FileNotFoundException>(
                () => _fileService.DeleteFileAsync(nonExistentFileName)
            );
        }
        [Fact]
        public async Task DeleteFileNullUrl()
        {
            await Assert.ThrowsAsync<ArgumentException>(
                () => _fileService.DeleteFileAsync(null)
            );
        }

        //Helper methods
        private IFormFile CreateMockFormFile(string fileName, byte[] fileContent, string contentType)
        {
            var mockFormFile = new Mock<IFormFile>();
            var memoryStream = new MemoryStream(fileContent);

            mockFormFile.Setup(f => f.FileName).Returns(fileName);
            mockFormFile.Setup(f => f.Length).Returns(fileContent.Length);
            mockFormFile.Setup(f => f.OpenReadStream()).Returns(memoryStream);
            mockFormFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns((Stream s, CancellationToken ct) =>
                {
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    return memoryStream.CopyToAsync(s, 81920, ct);
                });
            mockFormFile.Setup(f => f.ContentType).Returns(contentType);

            return mockFormFile.Object;
        }

        public void Dispose()
        {
            // Cleanup test files
            try
            {
                if (Directory.Exists(_testDirectory))
                    Directory.Delete(_testDirectory, true);
            }
            catch { }
        }
    }
}
