using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities.Users;
using Gym_App.Service.Functions.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Gym_App.Service.Functions.The_Applied
{
    public class UserF : IUserF
    {
        private readonly DbBase _db;
        public UserF(DbBase db) {
            _db = db;
        }
        public async Task<int> SignUpUser(UserDTO u)//0 == missing Information. 1 == Name already in use. 2 == Email is in use. 3 == Email not valid. 4 == Password not valid. 
            //5 == Succesful signup
        {
            if(u.Name == null || u.Email == null || u.Password == null)return await Task.FromResult(0);
            if (!isNameValid(u.Name)) return await Task.FromResult(1);
            if (EmailExists(u.Email)) return await Task.FromResult(2);
            if(!IsEmailValid(u.Email)) return await Task.FromResult(3);
            if(!IsPasswordValid(u.Password).Result) return await Task.FromResult(4);
            var user = new Trainee
            {
                UserID = Guid.NewGuid(),
                Name = u.Name,
                Email = u.Email,
                Password = new PasswordHasher<UserDTO>().HashPassword(u, u.Password),
                CreatedAt = DateTime.Now
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return await Task.FromResult(5);
        }

        public Task<int> LoginUser(UserDTO u) // 0 ==  mail not found. 1 == password is wrong . 2 == succesful login
        {
            var _user = (from user in _db.Users
                         where user.Email.ToLower() == u.Email.ToLower()
                         select user).FirstOrDefault();
            if (_user is null) return Task.FromResult(0);
            var result = new PasswordHasher<User>().VerifyHashedPassword(_user, _user.Password, u.Password);
            if (result == PasswordVerificationResult.Success) return Task.FromResult(2);
            else
            {
                return Task.FromResult(1);
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
