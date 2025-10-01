using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;
using Gym_App.Service.Functions.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Gym_App.Service.Functions.The_Applied
{
    public class TokenHandler : ITokenHandler
    {
        private readonly DbBase _db;
        private readonly IConfiguration _config;
        public TokenHandler(DbBase db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }
        public Task<string> CreateAccessToken(UserDTO u) // For creating access Tokens
        {
            
            var claims = new List<Claim>
            {
                new Claim("name", u.Name),
                new Claim("email", u.Email),
                new Claim("userId",u.UserID.ToString())
                //new Claim(ClaimTypes.Role,u.Role)
            };
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config.GetValue<string>("JwtSettings:Token")!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var TokenDescriptor = new JwtSecurityToken(
                issuer: _config.GetValue<string>("JwtSettings:Issuer"),
                audience: _config.GetValue<string>("JwtSettings:Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddDays(2),//EXPIRATION DATE
                signingCredentials: creds
                );
            return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(TokenDescriptor));
        }
        public Task<string> CreateRefreshToken(Guid UserID)//For creating new RefreshTokens
        {
            
            var refreshToken = new RefreshTokens
            {
                RefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.Now.AddDays(4),
                UserID = UserID,
            };
            return Task.FromResult(refreshToken.RefreshToken);
        }
        public async Task<string>? RefreshingToken(Guid UserID) //For logging in
        {
            var isTokenExists = (from token in _db.RefreshTokens
                                 where token.UserID == UserID
                                 select token).FirstOrDefault();
            if (isTokenExists == null) return null;
            else
            {
                isTokenExists.Expires = DateTime.UtcNow.AddDays(4);
                isTokenExists.RefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
                _db.RefreshTokens.Update(isTokenExists);
                await _db.SaveChangesAsync();
                return await Task.FromResult(isTokenExists.RefreshToken);
            }
        }

        public async Task<ResponseToken>? ValidateAccessToken(string Refreshtoken) // for logging in with Tokens
        {
            var result = _db.RefreshTokens.FirstOrDefault(t => t.RefreshToken == Refreshtoken);
            if (result == null || result.Expires < DateTime.UtcNow)
            {
                return null;
            }
            var user = (from u in _db.Users
                        where u.UserID == result.UserID
                        select new UserDTO
                        {
                            UserID = u.UserID,
                            Name = u.Name,
                            Email = u.Email,
                            Password = u.Password
                        }).FirstOrDefault();
            var Token = CreateAccessToken(user).Result;
            result.Expires = DateTime.UtcNow.AddDays(4);
            result.RefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var Response = new ResponseToken
            {
                Status = 1,
                AccessToken = Token,
                RefreshToken = result.RefreshToken,
            };
            _db.RefreshTokens.Update(result);
            await _db.SaveChangesAsync();
            return await Task.FromResult(Response);
        }
        public Task<IQueryable<RefreshTokens>> GetAllRefreshTokens()
        {
            var tokens = (from t in _db.RefreshTokens
                          select t).AsQueryable();
            return Task.FromResult(tokens);
        }
    }
}
