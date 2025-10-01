using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;
using Gym_App.Domain.Entities.Users;
using Gym_App.Service.Functions.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Gym_App.Service.Functions.The_Applied
{
    public class UserF : IUserF
    {
        private readonly DbBase _db;
        private readonly ITokenHandler _tokenHandler;
        public UserF(DbBase db, ITokenHandler tokenHandler)
        {
            _db = db;
            _tokenHandler = tokenHandler;
        }
        public async Task<ResponseToken> SignUpUser(UserDTO u)//0 == missing Information. 1 == Name already in use. 2 == Email is in use. 3 == Email not valid. 4 == Password not valid. 
            //5 == Succesful signup
        {
            //Signing up the user
            if (u.Name == null || u.Email == null || u.Password == null)return await Task.FromResult(new ResponseToken { Status = 0 });
            if (!isNameValid(u.Name)) return await Task.FromResult(new ResponseToken { Status = 1});
            if (EmailExists(u.Email)) return await Task.FromResult(new ResponseToken { Status = 2 });
            if(!IsEmailValid(u.Email)) return await Task.FromResult(new ResponseToken { Status = 3 });
            if(!IsPasswordValid(u.Password).Result) return await Task.FromResult(new ResponseToken { Status = 4 });
            var user = new Trainee
            {
                UserID = Guid.NewGuid(),
                Name = u.Name,
                Email = u.Email,
                Password = new PasswordHasher<UserDTO>().HashPassword(u, u.Password),
                CreatedAt = DateTime.Now
            };
            //Making the Tokens and saving them to the database
            u.UserID = user.UserID;
            var Token = await _tokenHandler.CreateAccessToken(u);
            var RefreshToken = await _tokenHandler.CreateRefreshToken(user.UserID);
            _db.Users.Add(user);
            _db.RefreshTokens.Add(new RefreshTokens
            {
                UserID = user.UserID,
                RefreshToken = RefreshToken,
                Expires = DateTime.Now.AddDays(4)
            });
            await _db.SaveChangesAsync();
            return await Task.FromResult(new ResponseToken {
                Status = 5,
                AccessToken = Token,
                RefreshToken = RefreshToken
            });
        }

        public async Task<ResponseToken> LoginUser(UserDTO u) // 0 ==  mail not found. 1 == password is wrong . 2 == succesful login
        {   //Checking if the user exists
            var _user = (from user in _db.Users
                         where user.Email.ToLower() == u.Email.ToLower()
                         select user).FirstOrDefault();
            if (_user is null) return await Task.FromResult(new ResponseToken { Status=0});
            var result = new PasswordHasher<User>().VerifyHashedPassword(_user, _user.Password, u.Password);
            if (result == PasswordVerificationResult.Failed) return await Task.FromResult(new ResponseToken { Status = 1  });
            else //Successful login and returning new Tokens
            {
                var RefreshToken = await _tokenHandler.RefreshingToken(_user.UserID); 
                return await Task.FromResult(new ResponseToken { 
                    Status = 2 ,
                    RefreshToken = RefreshToken
                });
            }
        }
        public Task<IQueryable<UserDTO>> GetAllUsers()
        {
            var users = (from u in _db.Users
                         select new UserDTO
                         {
                             UserID = u.UserID,
                             Name = u.Name,
                             Email = u.Email,
                             Password = u.Password
                         }).AsQueryable();
            return Task.FromResult(users);
        }
        public bool isNameValid(string name)//checks if the name is already taken
        {
            var n = (from u in _db.Users
                     where u.Name.ToLower() == name.ToLower()
                     select u).FirstOrDefault();

            if (n is null) return true;
            else return false;
        }
        public bool EmailExists(string email)
        {
            return _db.Users.Any(u => u.Email.ToLower() == email.ToLower());
        }
        public bool IsEmailValid(string email)
        {
            if (new EmailAddressAttribute().IsValid(email) && email != null) 
            {
                return true;
            }
            else return false;
        }
        public async Task<bool> IsPasswordValid(string password)
        {
            var PasswordPolicy = new Microsoft.AspNet.Identity.PasswordValidator
            {
                RequiredLength = 8,
                RequireNonLetterOrDigit = false,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true,
            };
            var result = await PasswordPolicy.ValidateAsync(password);
            if(result.Succeeded) return true;
            else return false;
        }
        public async Task<bool> DeleteUser(Guid userID)
        {
            var u = (from usr in _db.Users
                     where usr.UserID == userID
                     select usr).FirstOrDefault();
            if (u is null) return await Task.FromResult(false);
            _db.Users.Attach(u);
            _db.Users.Remove(u);
            await _db.SaveChangesAsync();
            return await Task.FromResult(true);
        }
    }
}
