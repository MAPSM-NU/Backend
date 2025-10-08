using DocumentFormat.OpenXml.Office2016.Excel;
using Gym_App.Domain.DTOs;
using Gym_App.Domain.Entities;
using Gym_App.Service.Functions.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace Gym_App.Service.Functions.The_Applied
{
    public class UserService : IUserServise
    {
        private readonly DbBase _db;
        private readonly ITokenHandler _tokenHandler;
        public UserService(DbBase db, ITokenHandler tokenHandler)
        {
            _db = db;
            _tokenHandler = tokenHandler;
        }
        public async Task<ResponseToken> SignUpUser(UserDTO u)//0 == missing Information. 1 == Name already in use. 2 == Email is in use. 3 == Email not valid. 4 == Password not valid. 
                                                              //5 == Succesful signup
        {
            //Signing up the user
            if (u.Name == null || u.Email == null || u.Password == null) return await Task.FromResult(new ResponseToken { Status = 0 });
            if (!isNameValid(u.Name)) return await Task.FromResult(new ResponseToken { Status = 1 });
            if (EmailExists(u.Email)) return await Task.FromResult(new ResponseToken { Status = 2 });
            if (!IsEmailValid(u.Email)) return await Task.FromResult(new ResponseToken { Status = 3 });
            if (!IsPasswordValid(u.Password).Result) return await Task.FromResult(new ResponseToken { Status = 4 });
            if (u.UserType == null) u.UserType = "Trainee";

            User user;
            if (u.UserType == "Trainer" || u.UserType.ToLower() == "t")
            {
                user = new User
                {
                    UserID = Guid.NewGuid(),
                    Name = u.Name,
                    Email = u.Email,
                    Password = new PasswordHasher<User>().HashPassword(null, u.Password),
                    CreatedAt = DateTime.Now,
                    UserType = "Trainee"
                };
            }
            else if(u.UserType.ToLower() == "coach" || u.UserType.ToLower() == "c")
            {
                user = new User
                {
                    UserID = Guid.NewGuid(),
                    Name = u.Name,
                    Email = u.Email,
                    Password = new PasswordHasher<User>().HashPassword(null, u.Password),
                    CreatedAt = DateTime.Now,
                    UserType = "Coach"
                };
            }
            else if (u.UserType.ToLower() == "doctor" || u.UserType.ToLower() == "d")
            {
                user = new User
                {
                    UserID = Guid.NewGuid(),
                    Name = u.Name,
                    Email = u.Email,
                    Password = new PasswordHasher<User>().HashPassword(null, u.Password),
                    CreatedAt = DateTime.Now,
                    UserType = "Doctor"
                };
            }
            else
            {
                return await Task.FromResult(new ResponseToken { Status = 7 });
            }

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
            return await Task.FromResult(new ResponseToken
            {
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
            if (_user is null) return await Task.FromResult(new ResponseToken { Status = 0 });
            var result = new PasswordHasher<User>().VerifyHashedPassword(_user, _user.Password, u.Password);
            if (result == PasswordVerificationResult.Failed) return await Task.FromResult(new ResponseToken { Status = 1 });
            else //Successful login and returning new Tokens
            {
                var RefreshToken = await _tokenHandler.RefreshingToken(_user.UserID);
                return await Task.FromResult(new ResponseToken
                {
                    Status = 2,
                    RefreshToken = RefreshToken
                });
            }
        }
        public async Task<int> UpdateUser(UserUpdateDTO User)
        {
            var user = (from u in _db.Users
                        where u.UserID == User.UserID
                        select u).FirstOrDefault();
            if (user is null) return await Task.FromResult(0);
            if (User.Name != null)
            {
                if (!isNameValid(User.Name)) return await Task.FromResult(2);
                user.Name = User.Name;
            }
            if (User.Bio != null) user.Bio = User.Bio;
            if (User.DOB > user.DOB) user.DOB = (DateTime)User.DOB;
            if (User.State != null) user.State = User.State;
            if (User.City != null) user.City = User.City;
            if (User.Country != null) user.Country = User.Country;
            if (User.PhoneNumber != null) user.PhoneNumber = User.PhoneNumber;
            if (User.ProfilePictureUrl != null) user.ProfilePictureUrl = User.ProfilePictureUrl;
            if (User.HeightCm != null) user.HeightCm = User.HeightCm;
            if (User.WeightKg != null) user.WeightKg = User.WeightKg;
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
            return await Task.FromResult(1);
        }
        public async Task<int> ChangeUserType(UserTypeDTO User)
        {
            var user = (from u in _db.Users
                        where u.UserID == User.UserID
                        select u).FirstOrDefault();
            if (user is null) return await Task.FromResult(0);
            if ((user.UserType.ToLower() == "coach" || user.UserType.ToLower() == "c" || user.UserType.ToLower() == "doctor" || user.UserType.ToLower() == "d") && User.UserType.ToLower() == "trainee")
            {
                user.UserType = "Trainee";
                user.Specialty = null;
                user.ExperienceYears = null;
                user.Certifications = null;
            }
            else
            {
                var usertype = ((User.UserType.ToLower() == "coach"|| User.UserType.ToLower() == "c") ? "Coach" : "Doctor");//needs change
                user.UserType = usertype ;
                user.Specialty = User.Specialty;
                user.ExperienceYears = User.ExperienceYears;
                user.Certifications = User.Certifications;
                
            }
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
            return await Task.FromResult(1);
        }
        public async Task<IQueryable<User>> GetAllUsers()
        {
            var users = from u in _db.Users.Include(us => us.Workouts)
                                           .Include(us => us.Schedules)
                        select u;
            return await Task.FromResult(users);
        }
        // Helper Functions
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
            if (result.Succeeded) return true;
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
