using DocumentFormat.OpenXml.Office2016.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Gym_App.Application.Interfaces;
using Gym_App.Domain;
using Gym_App.Domain.Entities;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.DTOs.UserDTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

namespace Gym_App.Application.Services
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
        public async Task<ResponseToken> CreateAdmin(UserCreationDTO u)
        {
            //Creating an admin user
            if (u == null || u.Name == null || u.Email == null || u.Password == null)
                return new ResponseToken { Status = 0 };

            if (!await isNameValid(u.Name))
                return new ResponseToken { Status = 1 };

            if (await EmailExists(u.Email))
                return new ResponseToken { Status = 2 };

            if (!IsEmailValid(u.Email))
                return new ResponseToken { Status = 3 };

            if (!IsPasswordValid(u.Password).Result)
                return new ResponseToken { Status = 4 };

            var role = await _db.Roles.FirstOrDefaultAsync(r => r.RoleName == "Admin");
            if (role == null)
                return new ResponseToken { Status = 6 };

            User user = new User
            {
                UserID = Guid.NewGuid(),
                Name = u.Name,
                Email = u.Email,
                Password = new PasswordHasher<User>().HashPassword(null, u.Password),
                CreatedAt = DateTime.Now,
                UserType = "Admin",
                Role = role
            };

            var Token = await _tokenHandler.CreateAccessToken(user.UserID, user.Name, user.Email, user.Role.RoleName);

            var RefreshToken = await _tokenHandler.CreateRefreshToken(user.UserID);

            await _db.Users.AddAsync(user);
            await _db.RefreshTokens.AddAsync(new RefreshTokens
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
        public async Task<ResponseToken> SignUpUser(UserCreationDTO u)//0 == missing Information. 1 == Name already in use. 2 == Email is in use. 3 == Email not valid. 4 == Password not valid. 
                                                                      //5 == Succesful signup
        {
            //Signing up the user
            if (u.Name == null || u.Email == null || u.Password == null)
                return new ResponseToken { Status = 0 };

            if (!await isNameValid(u.Name))
                return new ResponseToken { Status = 1 };

            if (await EmailExists(u.Email))
                return new ResponseToken { Status = 2 };

            if (!IsEmailValid(u.Email))
                return new ResponseToken { Status = 3 };

            if (!await IsPasswordValid(u.Password))
                return new ResponseToken { Status = 4 };

            if (u.UserType == null)
                u.UserType = "Trainee";

            var role = await _db.Roles.FirstOrDefaultAsync(r => r.RoleName == "User");
            if (role == null)
                return new ResponseToken { Status = 6 };

            User user = new User
            {
                UserID = Guid.NewGuid(),
                Name = u.Name,
                Email = u.Email,
                Password = new PasswordHasher<User>().HashPassword(null, u.Password),
                CreatedAt = DateTime.Now,
                Role = role
            };

            if (u.UserType.ToLower() == "coach" || u.UserType.ToLower() == "c")
                user.UserType = "Coach";

            else if (u.UserType.ToLower() == "doctor" || u.UserType.ToLower() == "d")
                user.UserType = "Doctor";

            else
                user.UserType = "Trainee";

            var Token = await _tokenHandler.CreateAccessToken(user.UserID, user.Name, user.Email, user.Role.RoleName);
            var RefreshToken = await _tokenHandler.CreateRefreshToken(user.UserID);


            await _db.Users.AddAsync(user);
            await _db.RefreshTokens.AddAsync(new RefreshTokens
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
        public async Task<ResponseToken> SigninUser(string email,string password) // 0 ==  mail not found. 1 == password is wrong . 2 == succesful login
        {   //Checking if the user exists
            var isUserExists = await(from u in _db.Users.Include(u=>u.Role)
                                     where u.Email.ToLower() == email.ToLower()
                                     select u).FirstOrDefaultAsync();

            if (isUserExists is null)
                return new ResponseToken { Status = 0 };

            var result = new PasswordHasher<User>().VerifyHashedPassword(isUserExists, isUserExists.Password, password);
            if (result == PasswordVerificationResult.Failed)
                return new ResponseToken { Status = 1 };
            else //Successful login and returning new Tokens
            {
                var AccessToken = await _tokenHandler.CreateAccessToken(isUserExists.UserID, isUserExists.Name, isUserExists.Email, isUserExists.Role.RoleName);
                
                var RefreshToken = await _tokenHandler.RefreshingToken(isUserExists.UserID);
                
                return new ResponseToken
                {
                    Status = 2,
                    AccessToken = AccessToken,
                    RefreshToken = RefreshToken
                };
            }
        }
        public async Task<int> UpdateUser(UserUpdateDTO User)
        {
            var user = await(from u in _db.Users
                        where u.UserID == User.UserID
                        select u).FirstOrDefaultAsync();
            if (user is null) return 0;
            if (User.Name != null)
            {
                if (!await isNameValid(User.Name)) return 1;
                user.Name = User.Name;
            }
            
            if (User.Bio != null)
                user.Bio = User.Bio;
            
            if (User.DOB > user.DOB)
                user.DOB = User.DOB;
            
            if (User.State != null)
                user.State = User.State;
            
            if (User.City != null)
                user.City = User.City;
            
            if (User.Country != null)
                user.Country = User.Country;
            
            if (User.PhoneNumber != null)
                user.PhoneNumber = User.PhoneNumber;
            
            if (User.ProfilePictureUrl != null)
                user.ProfilePictureUrl = User.ProfilePictureUrl;
            
            if (User.HeightCm != null)
                user.HeightCm = User.HeightCm;
            
            if (User.WeightKg != null) 
                user.WeightKg = User.WeightKg;
            
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
            return 2;
        }
        public async Task<int> ChangeUserType(UserChangeTypeDTO User)
        {
            if(User.UserType == null)
                return 0;
            
            string keywords = "coach, c, doctor, d, trainee, t";
            
            if (!keywords.Contains(User.UserType.ToLower()))return 1;
            
            var user = await(from u in _db.Users
                        where u.UserID == User.UserID
                        select u).FirstOrDefaultAsync();
            
            if (user is null)
                return 2;
            
            var usertype = user.UserType.ToLower();
            
            var incomingUsertype = User.UserType.ToLower();
            
            if(incomingUsertype == "t")
                incomingUsertype = "trainee";
            
            if(incomingUsertype == "c")
                incomingUsertype = "coach";
            
            if(incomingUsertype == "d")
                incomingUsertype = "doctor";
            
            if (usertype == incomingUsertype) return 3;//same user type
            else
            {
                user.UserType = incomingUsertype;
                if (incomingUsertype == "coach" || incomingUsertype == "doctor")// could maybe make the trainne doctor and coach be t,d,c to make the comparisons less computational
                {
                    user.Specialty = User.Specialty;
                    user.ExperienceYears = User.ExperienceYears;
                    user.Certifications = User.Certifications;
                }
                else
                {
                    user.Specialty = null;
                    user.ExperienceYears = null;
                    user.Certifications = null;
                }
            }

            _db.Users.Update(user);
            await _db.SaveChangesAsync();
            return 4;
        }
        public async Task<bool> DeleteUser(Guid userID)
        {
            var u = await(from usr in _db.Users
                     where usr.UserID == userID
                     select usr).FirstOrDefaultAsync();
            if (u is null) return false;
            _db.Users.Attach(u);
            _db.Users.Remove(u);
            await _db.SaveChangesAsync();
            return true;
        }
        public async Task<UserViewDTO?> GetUserByID(Guid userID)
        {
            var user = await(from u in _db.Users
                        where u.UserID == userID
                        select new UserViewDTO
                        {
                            UserID = userID,
                            Name = u.Name,
                            Email = u.Email,
                            Bio = u.Bio,
                            CreatedAt = u.CreatedAt,
                            DOB = u.DOB,
                            State = u.State,
                            City = u.City,
                            Country = u.Country,
                            PhoneNumber = u.PhoneNumber,
                            ProfilePictureUrl = u.ProfilePictureUrl,
                            subscriptionPlan = u.subscriptionPlan,
                            HeightCm = u.HeightCm,
                            WeightKg = u.WeightKg,
                            UserType = u.UserType
                        }).FirstOrDefaultAsync();
            if (user is null) return null;
            return user;
        }
        public async Task<PagedList<UserSmallViewDTO>?> GetUsersByFilter(string startDate,string endDate,int page, string sortColumn, string OrderBy, string searchTerm, int pageSize)
        {
            if (page == 0) page = 1;
            if (pageSize == 0) pageSize = 10;
            IQueryable<User> userQuery = _db.Users;
            DateTime validStartDate, validEndDate;
            if(DateTime.TryParse(startDate,out validStartDate))
            {
                userQuery = userQuery.Where(u => u.CreatedAt > validStartDate);
            }
            if(DateTime.TryParse(endDate, out validEndDate))
            {
                userQuery = userQuery.Where(u=>u.CreatedAt < validEndDate);
            }
            if (!string.IsNullOrEmpty(searchTerm)) userQuery = userQuery.Where(u => u.Name.Contains(searchTerm) || u.Email.Contains(searchTerm) || u.Specialty.Contains(searchTerm));
            
            if (!string.IsNullOrEmpty(sortColumn))
            {
                Expression<Func<User, object>> keySelector = sortColumn.ToLower() switch // throws error when sortColumn is null
                {
                    "name" or "n" => User => User.Name,
                    "email" or "e" => User => User.Email,
                    "country" or "co" => User => User.Country,
                    "state" or "s" => User => User.State,
                    "city" or "ci" => User => User.City,
                    "height" or "h" => User => User.HeightCm,
                    "weight" or "w" => User => User.WeightKg,
                    _ => User => User.UserID,
                };
                if(!string.IsNullOrEmpty(OrderBy))userQuery = userQuery.OrderBy(keySelector);
                else userQuery = userQuery.OrderBy(keySelector);
            }
            var userResponse = userQuery
                                .Select(u => new UserSmallViewDTO
                                {
                                    UserID = u.UserID,
                                    Name = u.Name,
                                    Email = u.Email,
                                    ProfilePictureUrl = u.ProfilePictureUrl
                                });
            var users = await PagedList<UserSmallViewDTO>.CreateAsync(userResponse, page, pageSize);
            return users;
        }
        public async Task<PagedList<UserViewDTO>?> GetAllUsers(int page, int pageSize)
        {
            if (page == 0) page = 1;
            if (pageSize == 0) pageSize = 10;
            var usersQuery =   from u in _db.Users
                               select new UserViewDTO
                               {
                                   UserID = u.UserID,
                                   Name = u.Name,
                                   Email = u.Email,
                                   Bio = u.Bio,
                                   CreatedAt = u.CreatedAt,
                                   DOB = u.DOB,
                                   State = u.State,
                                   City = u.City,
                                   Country = u.Country,
                                   PhoneNumber = u.PhoneNumber,
                                   ProfilePictureUrl = u.ProfilePictureUrl,
                                   subscriptionPlan = u.subscriptionPlan,
                                   HeightCm = u.HeightCm,
                                   WeightKg = u.WeightKg,
                                   UserType = u.UserType
                               };
            var users = await PagedList<UserViewDTO>.CreateAsync(usersQuery,page,pageSize);
            return users;
        }


        //Helper Functions
        //-----------------

        public async Task<bool> isNameValid(string name)//checks if the name is already taken
        {
            var n = await(from u in _db.Users
                     where u.Name.ToLower() == name.ToLower()
                     select u).FirstOrDefaultAsync();

            if (n is null) return true;
            else return false;
        }
        public async Task<bool> EmailExists(string email)
        {
            return await _db.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
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
    }
}
