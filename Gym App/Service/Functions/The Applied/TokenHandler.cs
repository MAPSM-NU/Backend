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
        public Task<string> CreateAccessToken(UserDTO u)
        {
            Guid userID = (from usr in _db.Users
                           where usr.Name == u.Name
                           select u.UserID).FirstOrDefault();
            var claims = new List<Claim>
            {
                new Claim("name", u.Name),
                new Claim("email", u.Email),
                new Claim("userId",userID.ToString())
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
        public async Task<string> CreateRefreshToken(UserDTO u)//could be optimized more
        {
            Guid userID = (from usr in _db.Users
                       where usr.Name == u.Name
                       select usr.UserID).FirstOrDefault();
            var refreshToken = new RefreshTokens
            {
                RefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.Now.AddDays(4),
                UserID = userID,
            };
            _db.RefreshTokens.Add(refreshToken);
            await _db.SaveChangesAsync();
            return await Task.FromResult(refreshToken.RefreshToken);
        }

        public async Task<Response>? ValidateAccessToken(string Refreshtoken)
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
            var Response = new Response
            {
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
