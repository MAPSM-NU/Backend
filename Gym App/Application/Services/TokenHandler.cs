using Gym_App.Application.Interfaces;
using Gym_App.Domain;
using Gym_App.Domain.Entities;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.DTOs.UserDTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Gym_App.Application.Services
{
    public class TokenHandler : ITokenHandler
    {
        private readonly DbBase _db;
        private readonly IConfiguration _config;
        //private readonly ILogger _logger;
        public TokenHandler(DbBase db, IConfiguration config)
        {
            _db = db;
            _config = config;
            //_logger = logger;
        }
        
        //        *********** Setters ***********
        public async Task<string> CreateAccessToken(Guid userID, string name,string email, string role) // For creating access Tokens
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Name, name),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(JwtRegisteredClaimNames.Sub,userID.ToString()),
                new Claim(ClaimTypes.Role,role)
            };
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config.GetValue<string>("JwtSettings:Token")!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var TokenDescriptor = new JwtSecurityToken(
                issuer: _config.GetValue<string>("JwtSettings:Issuer"),
                audience: _config.GetValue<string>("JwtSettings:Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),//EXPIRATION DATE
                signingCredentials: creds
                );
            //_logger.LogInformation("Json Web Token created");

            return await Task.FromResult(new JwtSecurityTokenHandler().WriteToken(TokenDescriptor));
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
        public async Task<string?> RefreshingToken(Guid UserID) //For logging in
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

        //-----------------------------------------------------------------------


        //        *********** Extra Validation Function ***********

        public async Task<ResponseToken?> ValidateAccessToken(string Refreshtoken) // for logging in with Tokens
        {
            var result = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.RefreshToken == Refreshtoken);
            if (result == null || result.Expires < DateTime.UtcNow)
            {
                return null;
            }
            var user = await (from u in _db.Users.Include(u=>u.Role)
                              where u.UserID == result.UserID
                              select u).FirstOrDefaultAsync();

            if(user == null)
                return null;

            var Token = await CreateAccessToken(user.UserID, user.Name, user.Email, user.Role.RoleName);

            result.Expires = DateTime.UtcNow.AddDays(4);
            result.RefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            var Response = new ResponseToken
            {
                Status = 1,
                AccessToken = Token,
                RefreshToken = result.RefreshToken,
                msg = "Token refreshed successfully"
            };

            _db.RefreshTokens.Update(result);
            await _db.SaveChangesAsync();
            return Response;
        }

        //-----------------------------------------------------------------------

        //        *********** Getters ***********
        public async Task<PagedList<RefreshTokens>> GetAllRefreshTokens(int page,int pageSize)
        {
            if (page == 0)
                page = 1;

            if (pageSize == 0)
                pageSize = 10;

            var tokensQuery = from t in _db.RefreshTokens
                          select t;

            var tokens = await PagedList<RefreshTokens>.CreateAsync(tokensQuery, page, pageSize);
            return tokens;
        }
    }
}
