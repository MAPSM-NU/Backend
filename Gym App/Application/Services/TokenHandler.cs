using Gym_App.Domain;
using Gym_App.Domain.Entities;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.Interfaces.Repositries;
using Gym_App.Infastructure.Interfaces.Services;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Gym_App.Application.Services
{
    public class TokenHandler : ITokenHandler
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;

        public TokenHandler(IUnitOfWork unitOfWork, IConfiguration config)
        {
            _unitOfWork = unitOfWork;
            _config = config;
        }

        //        *********** Setters ***********

        public async Task<string> CreateAccessToken(Guid userID, string name, string email, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Name, name),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(JwtRegisteredClaimNames.Sub, userID.ToString()),
                new Claim(ClaimTypes.Role, role)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config.GetValue<string>("JwtSettings:Token")!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var TokenDescriptor = new JwtSecurityToken(
                issuer: _config.GetValue<string>("JwtSettings:Issuer"),
                audience: _config.GetValue<string>("JwtSettings:Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return await Task.FromResult(new JwtSecurityTokenHandler().WriteToken(TokenDescriptor));
        }

        public async Task<string> CreateRefreshToken(Guid UserID)
        {
            var refreshToken = new RefreshTokens
            {
                RefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.Now.AddDays(4),
                UserID = UserID,
            };
            await _unitOfWork.Tokens.Create(refreshToken);
            return refreshToken.RefreshToken;
        }

        public async Task<string?> RefreshingToken(Guid UserID)
        {
            //Getting refresh token from repository
            var isTokenExists = await _unitOfWork.Tokens.GetRefreshTokenByUserId(UserID);
            
            if (isTokenExists == null)
                return null;

            //Updating the refresh token
            var newRefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            await _unitOfWork.Tokens.UpdateRefreshToken(UserID, newRefreshToken, DateTime.UtcNow.AddDays(4));

            return newRefreshToken;
        }

        //-----------------------------------------------------------------------

        //        *********** Extra Validation Function ***********

        public async Task<ResponseToken?> ValidateAccessToken(string refreshToken)
        {
            //Getting refresh token from repository
            var result = await _unitOfWork.Tokens.GetRefreshTokenByToken(refreshToken);
            
            if (result == null || result.Expires < DateTime.UtcNow)
                return null;

            //Getting user from repository
            var user = await _unitOfWork.Users.GetById(result.Id);

            if (user == null)
                return null;

            //Creating new access token
            var Token = await CreateAccessToken(user.Id, user.Name, user.Email, user.Role.RoleName);

            //Updating refresh token
            var newRefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            await _unitOfWork.Tokens.UpdateRefreshToken(user.Id, newRefreshToken, DateTime.UtcNow.AddDays(4));

            var Response = new ResponseToken
            {
                Status = 1,
                AccessToken = Token,
                RefreshToken = newRefreshToken,
                msg = "Token refreshed successfully"
            };

            return Response;
        }

        //-----------------------------------------------------------------------

        //        *********** Getters ***********

        public async Task<PagedList<RefreshTokens>> GetAllRefreshTokens(int page, int pageSize)
        {
            if (page == 0)
                page = 1;

            if (pageSize == 0)
                pageSize = 10;

            //Getting all tokens from repository
            var tokensQuery = _unitOfWork.Tokens.GetAll();

            var tokens = await PagedList<RefreshTokens>.CreateAsync(tokensQuery, page, pageSize);
            return tokens;
        }
    }
}
