using Gym_App.Infastructure.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GymApp.Tests.TokenTests
{
    public class JwtCreationTests : TestBase
    {
        private readonly ITokenHandler _tokenHandler;
        public JwtCreationTests() : base("TokenTestDatabase")
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
        public async Task CreateAccessTokenTest()
        {
            var user = CreateTestUser(CreateTestRole());
            var token = await _tokenHandler.CreateAccessToken(user.Id,user.Name,user.Email,user.Role.RoleName);
            Assert.NotNull(token);
            Assert.Equal(371, token.Length);
        }
        [Fact]
        public async Task AccessTokenValueAreRight()
        {
            var user = CreateTestUser(CreateTestRole());
            var token = await _tokenHandler.CreateAccessToken(user.Id, user.Name, user.Email, user.Role.RoleName);
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            Assert.NotNull(jwtToken);
            var nameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
            var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            Assert.Equal(user.Name, nameClaim);
            Assert.Equal(user.Email, emailClaim);
            Assert.Equal(user.Id.ToString(), subClaim);
            Assert.Equal(user.Role.RoleName, roleClaim);
        }
    }
}
