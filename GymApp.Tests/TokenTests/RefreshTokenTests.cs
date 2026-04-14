using Gym_App.Infastructure.Interfaces.Services;
using Microsoft.Extensions.Configuration;

namespace GymApp.Tests.TokenTests
{
    public class RefreshTokenTests : TestBase
    {
        private readonly ITokenHandler _tokenHandler;
        public RefreshTokenTests() : base("RefreshTokenTestDatabase")
        {
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection()
                .Build();
            config["JwtSettings:Token"] = "super-secret-long-keyyyyyyyyyyyyyyyy";
            config["JwtSettings:Issuer"] = "TestIssuer";
            config["JwtSettings:Audience"] = "TestAudience";
            _tokenHandler = new Gym_App.Application.Services.TokenHandler(_unitOfWork, config);
        }
        [Fact]
        public async Task CreateRefreshTokenTest()
        {
            var user = CreateTestUser(CreateTestRole());
            var refreshToken = await _tokenHandler.CreateRefreshToken(user.Id);
            Assert.NotNull(refreshToken);
            Assert.Equal(88, refreshToken.Length);
        }
        [Fact]
        public async Task RefreshingToken()
        {
            var user = CreateTestUser(CreateTestRole());
            var refreshToken = await _tokenHandler.CreateRefreshToken(user.Id);
            await _unitOfWork.SaveChangesAsync();
            var newAccessToken = await _tokenHandler.RefreshingToken(user.Id);
            Assert.NotNull(newAccessToken);
            Assert.Equal(88, newAccessToken.Length);
        }
        [Fact]
        public async Task RefreshTokenNotFound()
        {
            var user = CreateTestUser(CreateTestRole());
            var newAccessToken = await _tokenHandler.RefreshingToken(user.Id);
            Assert.Null(newAccessToken);
        }
        [Fact]
        public async Task RefreshTokenIsValid()
        {
            var user = CreateTestUser(CreateTestRole());
            var refreshToken = await _tokenHandler.CreateRefreshToken(user.Id);
            await _unitOfWork.SaveChangesAsync();
            var newAccessToken = await _tokenHandler.RefreshingToken(user.Id);
            Assert.NotNull(newAccessToken);
        }
        [Fact]
        public async Task ValidatingRefreshToken()
        {
            var user = CreateTestUser(CreateTestRole());
            await _unitOfWork.SaveChangesAsync();
            string token = await _tokenHandler.CreateRefreshToken(user.Id);
            await _unitOfWork.SaveChangesAsync();
            var response = await _tokenHandler.ValidateRefreshToken(token);
            Assert.Equal("Token refreshed successfully", response!.msg);
            Assert.Equal(1, response!.Status);
            Assert.NotNull(response!.RefreshToken);
            Assert.NotNull(response.AccessToken);
        }
        [Fact]
        public async Task RefreshTokenIsInvalidAfterUse()
        {
            var user = CreateTestUser(CreateTestRole());
            string token = await _tokenHandler.CreateRefreshToken(user.Id);
            await _unitOfWork.SaveChangesAsync();
            var refreshToken = await _unitOfWork.Tokens!.GetRefreshTokenByUserId(user.Id);
            Assert.NotNull(refreshToken);
            refreshToken.Expires = DateTime.UtcNow.AddSeconds(-1);
            await _unitOfWork.Tokens.Update(refreshToken);
            await _unitOfWork.SaveChangesAsync();
            var response = await _tokenHandler.ValidateRefreshToken(token);
            Assert.Equal("Refresh token has expired", response!.msg);
            Assert.Equal(0, response!.Status);
        }
        [Fact]
        public async Task ValidatingNotFoundToken()
        {
            var response = await _tokenHandler.ValidateRefreshToken("non-existent-token");
            Assert.Equal("Refresh token not found", response!.msg);
            Assert.Equal(0, response!.Status);
        }
        [Fact]
        public async Task RefreshTokenIsSavedInDatabase()
        {
            var user = CreateTestUser(CreateTestRole());
            var refreshToken = await _tokenHandler.CreateRefreshToken(user.Id);
            await _unitOfWork.SaveChangesAsync();
            var tokenInDb = _unitOfWork.Tokens!.GetAll().FirstOrDefault(t => t.RefreshToken == refreshToken);
            Assert.NotNull(tokenInDb);
            Assert.Equal(user.Id, tokenInDb.UserID);
        }
    }
}
