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
        public Task<bool> AddUser(UserDTO u)
        {
            if(u.Email == null || !IsEmailValid(u.Email)) return Task.FromResult(false);
            if(u.Name == null || !isNameValid(u.Name)) return Task.FromResult(false);
            var user = new User
            {
                Name = u.Name,
                Email = u.Email,
                Password = new PasswordHasher<UserDTO>().HashPassword(u, u.Password)
            };
            _db.Users.Add(user);
            _db.SaveChanges();
            return Task.FromResult(true);
        }

        public Task<bool> LoginUser(UserDTO u)
        {
            var _user = (from user in _db.Users
                         where user.Email == u.Email
                         select user).FirstOrDefault();
            if (_user is null) return Task.FromResult(false);
            var result = new PasswordHasher<User>().VerifyHashedPassword(_user, _user.Password, u.Password);
            if (result == PasswordVerificationResult.Success) return Task.FromResult(true);
            else
            {
                return Task.FromResult(false);
            }
        }
        public Task<IQueryable<UserDTO>> GetAllUsers()
        {
            var users = (from u in _db.Users
                         select new UserDTO
                         {
                             Name = u.Name,
                             Email = u.Email,
                             Password = u.Password
                         }).AsQueryable();
            return Task.FromResult(users);
        }

        public bool EmailExists(string email)
        {
            return _db.Users.Any(u => u.Email == email);
        }
        public bool IsEmailValid(string email)
        {
            if (!EmailExists(email) && new EmailAddressAttribute().IsValid(email) && email != null) 
            {
                return true;
            }
            else return false;
        }
        public bool isNameValid(string name)//checks if the name is already taken
        {
            var n = (from u in _db.Users
                     where u.Name == name
                     select u).FirstOrDefault();

            if (n is null) return true;
            else return false;
        }
    }
}
