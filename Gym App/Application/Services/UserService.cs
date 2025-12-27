using Gym_App.Application.Interfaces;
using Gym_App.Domain;
using Gym_App.Domain.Entities;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.DTOs.UserDTOs;
using Gym_App.Infastructure.Transfer_Classes;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

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

        //        *********** Setters ***********

        //0 == Error(Bad Request) || 1 == Unauthorized (Forbid) || 2 == Success (Ok)

        public async Task<ResponseToken> CreateAdmin(UserCreationDTO u)
        {
            //Creating an admin user
            if (u == null || u.Name == null || u.Email == null || u.Password == null)
                return new ResponseToken { Status = 0, msg = "Invalid Data" };

            //Checking for name Validity
            if (!await isNameValid(u.Name))
                return new ResponseToken { Status = 0, msg = "Name is already used" };
            //Checking if Email is valid or not
            if (!IsEmailValid(u.Email))
                return new ResponseToken { Status = 0, msg = "Invalid Email" };
            //Checking if the email already exists
            if (await EmailExists(u.Email))
                return new ResponseToken { Status = 0, msg = "Email already in use" };
            //Checking for password validity
            if (!IsPasswordValid(u.Password).Result)
                return new ResponseToken { Status = 0, msg = "Invalid Password" };

            //Creating the admin user
            var role = await _db.Roles.FirstOrDefaultAsync(r => r.RoleName == "Admin");
            if (role == null)
                return new ResponseToken { Status = 0, msg = "Role not found" };

            //Creating the user 
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

            //Generating Tokens
            var Token = await _tokenHandler.CreateAccessToken(user.UserID, user.Name, user.Email, user.Role.RoleName);
            var RefreshToken = await _tokenHandler.CreateRefreshToken(user.UserID);

            //Saving to Database
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
                Status = 1,
                AccessToken = Token,
                RefreshToken = RefreshToken,
                msg = "Admin created successfully"
            });
        }
        public async Task<ResponseToken> SignUpUser(UserCreationDTO u)//0 == missing Information. 1 == Name already in use. 2 == Email is in use. 3 == Email not valid. 4 == Password not valid. 
                                                                      //5 == Succesful signup
        {
            //Signing up the user
            if (u.Name == null || u.Email == null || u.Password == null)
                return new ResponseToken { Status = 0, msg = "Missing Information" };
            //Checking for name Validity
            if (!await isNameValid(u.Name))
                return new ResponseToken { Status = 0, msg = "Name is already used" };
            //Checking if Email is valid or not
            if (!IsEmailValid(u.Email))
                return new ResponseToken { Status = 0, msg = "Invalid Email" };
            //Checking if the email already exists
            if (await EmailExists(u.Email))
                return new ResponseToken { Status = 0, msg = "Email already in use" };
            //Checking for password validity
            if (!await IsPasswordValid(u.Password))
                return new ResponseToken { Status = 0, msg = "Invalid Password" };

            //Getting the role for the user
            var role = await _db.Roles.FirstOrDefaultAsync(r => r.RoleName == "User");
            if (role == null)
                return new ResponseToken { Status = 0, msg = "Role not found" };

            //Creating the user
            User user = new User
            {
                UserID = Guid.NewGuid(),
                Name = u.Name,
                Email = u.Email,
                Password = new PasswordHasher<User>().HashPassword(null, u.Password),
                CreatedAt = DateTime.Now,
                Role = role
            };

            //Setting UseeType to Coach
            if (u.UserType!.ToLower() == "coach" || u.UserType.ToLower() == "c")
                user.UserType = "Coach";
            //Setting UserType to Doctor
            else if (u.UserType.ToLower() == "doctor" || u.UserType.ToLower() == "d")
                user.UserType = "Doctor";
            //If not coach or Doctor set to Trainee
            else
                user.UserType = "Trainee";

            //Generating Tokens
            var Token = await _tokenHandler.CreateAccessToken(user.UserID, user.Name, user.Email, user.Role.RoleName);
            var RefreshToken = await _tokenHandler.CreateRefreshToken(user.UserID);

            //Saving to Database
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
                Status = 1,
                AccessToken = Token,
                RefreshToken = RefreshToken,
                msg = "User created successfully"
            });
        }
        public async Task<ResponseToken> SigninUser(string email, string password) // 0 ==  mail not found. 1 == password is wrong . 2 == succesful login
        {   //Checking if the user exists
            var isUserExists = await (from u in _db.Users.Include(u => u.Role)
                                      where u.Email.ToLower() == email.ToLower()
                                      select u).FirstOrDefaultAsync();
            //If User does not exist return
            if (isUserExists is null)
                return new ResponseToken { Status = 0, msg = "User not found" };

            //Checking password of found User
            var result = new PasswordHasher<User>().VerifyHashedPassword(isUserExists, isUserExists.Password, password);
            //If password does not match return
            if (result == PasswordVerificationResult.Failed)
                return new ResponseToken { Status = 0, msg = "Invalid Password" };
            //Successful login and returning new Tokens
            else
            {
                //Creating Tokens
                var AccessToken = await _tokenHandler.CreateAccessToken(isUserExists.UserID, isUserExists.Name, isUserExists.Email, isUserExists.Role.RoleName);
                var RefreshToken = await _tokenHandler.RefreshingToken(isUserExists.UserID);

                //Returning Tokens 
                return new ResponseToken
                {
                    Status = 1,
                    AccessToken = AccessToken,
                    RefreshToken = RefreshToken,
                    msg = "Login successful"
                };
            }
        }
        public async Task<SettersResponse> UpdateUser(UserUpdateDTO user)//0 == invalid user || 1 == user not found || 2 == name not valid || 3 == succesful update
        {
            if (user == null)
                return new SettersResponse { status = 0, msg = "Invalid user data." };
            //Getting the user from the database
            var isUserExists = await (from u in _db.Users
                                      where u.UserID == user.UserID
                                      select u).FirstOrDefaultAsync();
            //If user does not exist return
            if (isUserExists is null)
                return new SettersResponse { status = 0, msg = "User not found." };

            //If user wants to change name check if the name is valid
            if (!string.IsNullOrEmpty(user.Name))
            {
                //If name is not valid return
                if (!await isNameValid(user.Name))
                    return new SettersResponse { status = 0, msg = "Name is not valid." };
                //Else change to new name
                isUserExists.Name = user.Name;
            }
            //Updating Bio
            if (!string.IsNullOrEmpty(user.Bio))
                isUserExists.Bio = user.Bio;
            //Updating DOB
            if (user.DOB.ToString() != null)
                isUserExists.DOB = user.DOB;
            //Updating State 
            if (!string.IsNullOrEmpty(user.State))
                isUserExists.State = user.State;
            //Updating City
            if (!string.IsNullOrEmpty(user.City))
                isUserExists.City = user.City;
            //Updating Country
            if (!string.IsNullOrEmpty(user.Country))
                isUserExists.Country = user.Country;
            //Updating PhoneNumber
            if (!string.IsNullOrEmpty(user.PhoneNumber))
                isUserExists.PhoneNumber = user.PhoneNumber;
            //Updating Profile Picture
            if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
                isUserExists.ProfilePictureUrl = user.ProfilePictureUrl;
            //Updating Height
            if (user.HeightCm > 0)
                isUserExists.HeightCm = user.HeightCm;
            //Updating Weight
            if (user.WeightKg > 0)
                isUserExists.WeightKg = user.WeightKg;

            //Saving to Database
            _db.Users.Update(isUserExists);
            await _db.SaveChangesAsync();
            return new SettersResponse { status = 1, msg = "User updated successfully." };
        }
        public async Task<SettersResponse> ChangeUserType(UserChangeTypeDTO User)//0 == Faulty UserType || 1 == invalid UserType || 2 == user not found || 3 == same user type || 4 == succesful change
        {
            //Checking UserType validity
            if (User == null || User.UserType == null)
                return new SettersResponse { status = 0, msg = "Invalid user type." };

            //Keywords to check for UserType 
            string keywords = "coach, c, doctor, d, trainee, t";
            //If Usertype is none of the keywords return
            if (!keywords.Contains(User.UserType.ToLower()))
                return new SettersResponse { status = 0, msg = "Invalid user type." };

            //Getting User from Database
            var user = await (from u in _db.Users
                              where u.UserID == User.UserID
                              select u).FirstOrDefaultAsync();
            //If user not found return
            if (user is null)
                return new SettersResponse { status = 0, msg = "Invalid user data." };

            //Making all usertypes into lowercase letters
            var usertype = user.UserType.ToLower();
            var incomingUsertype = User.UserType.ToLower();

            // Changing userType from initials to full words to immedietly apply them to the user
            if (incomingUsertype == "t")
                incomingUsertype = "trainee";

            if (incomingUsertype == "c")
                incomingUsertype = "coach";

            if (incomingUsertype == "d")
                incomingUsertype = "doctor";

            if (usertype == incomingUsertype)
                return new SettersResponse { status = 0, msg = "Same user type" };//same user type
            else
            {
                //Changing from Trainee to either doctor or coach
                user.UserType = incomingUsertype;
                if (incomingUsertype == "coach" || incomingUsertype == "doctor")// could maybe make the trainne doctor and coach be t,d,c to make the comparisons less computational
                {
                    user.Specialty = User.Specialty;
                    user.ExperienceYears = User.ExperienceYears;
                    user.Certifications = User.Certifications;
                }
                //Changing from coach or doctor to trainee
                else
                {
                    user.Specialty = null;
                    user.ExperienceYears = null;
                    user.Certifications = null;
                }
            }

            //Saving to Database
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
            return new SettersResponse { status = 1, msg = "User type changed successfully." };
        }
        public async Task<SettersResponse> DeleteUser(Guid userID)//0 == error || 1 == successfull
        {
            //Checking userID validity
            if (userID == Guid.Empty)
                return new SettersResponse { status = 0, msg = "Invalid user ID." };

            //Getting user from database
            var u = await (from usr in _db.Users
                           where usr.UserID == userID
                           select usr).FirstOrDefaultAsync();
            //If user not found return
            if (u == null)
                return new SettersResponse { status = 0, msg = "User not found." };

            //Saving to Database
            _db.Users.Remove(u);
            await _db.SaveChangesAsync();
            return new SettersResponse { status = 1, msg = "User deleted successfully." };
        }

        //-----------------------------------------------------------------------


        //        *********** Extra Validation Function ***********

        public async Task<bool> isNameValid(string name)//checks if the name is already taken
        {
            var n = await (from u in _db.Users
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

        //-----------------------------------------------------------------------

        //        *********** Getters ***********

        public async Task<GettersResponse<UserViewDTO>> GetUserByID(Guid userID)
        {
            var user = await (from u in _db.Users
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
            if (user == null)
                return new GettersResponse<UserViewDTO>
                {
                    status = 0,
                    msg = "User not found"
                };
            else
                return new GettersResponse<UserViewDTO>
                {
                    status = 2,
                    msg = "Successful",
                    Value = user
                }; ;
        }
        public async Task<GettersResponse<UserMiniViewDTO>> GetMiniUsers(string startDate, string endDate, int page, string sortColumn, string OrderBy, string searchTerm, int pageSize)
        {
            IQueryable<User> userQuery = _db.Users;

            if (userQuery == null || userQuery.Count() == 0)
                return new GettersResponse<UserMiniViewDTO>
                {
                    status = 0,
                    msg = "no user to be found"
                };

            DateTime validStartDate, validEndDate;
            if(DateTime.TryParse(startDate,out validStartDate))
            {
                userQuery = userQuery.Where(u => u.CreatedAt > validStartDate);
            }
            if(DateTime.TryParse(endDate, out validEndDate))
            {
                userQuery = userQuery.Where(u=>u.CreatedAt < validEndDate);
            }
            if (!string.IsNullOrEmpty(searchTerm)) userQuery = userQuery.Where(u => u.Name.Contains(searchTerm) || u.Email.Contains(searchTerm) || u.Specialty!.Contains(searchTerm));
            
            if (!string.IsNullOrEmpty(sortColumn))
            {
                Expression<Func<User, object>> keySelector = sortColumn.ToLower() switch // throws error when sortColumn is null
                {
                    "name" or "n" => User => User.Name,
                    "email" or "e" => User => User.Email,
                    "country" or "co" => User => User.Country!,
                    "state" or "s" => User => User.State!,
                    "city" or "ci" => User => User.City!,
                    "height" or "h" => User => User.HeightCm!,
                    "weight" or "w" => User => User.WeightKg!,
                    _ => User => User.UserID,
                };
                if(!string.IsNullOrEmpty(OrderBy))userQuery = userQuery.OrderBy(keySelector);
                else userQuery = userQuery.OrderBy(keySelector);
            }
            var userResponse = userQuery
                                .Select(u => new UserMiniViewDTO
                                {
                                    UserID = u.UserID,
                                    Name = u.Name,
                                    Email = u.Email,
                                    ProfilePictureUrl = u.ProfilePictureUrl
                                });
            var users = await PagedList<UserMiniViewDTO>.CreateAsync(userResponse, page, pageSize);
            return new GettersResponse<UserMiniViewDTO>
            {
                status = 2,
                msg = "Successfull",
                Data = users
            };
        }
        public async Task<GettersResponse<UserViewDTO>> GetUsers(string startDate, string endDate, int page, string sortColumn, string OrderBy, string searchTerm, int pageSize)
        {
            IQueryable<User> userQuery = _db.Users;

            if (userQuery == null || userQuery.Count() == 0)
                return new GettersResponse<UserViewDTO>
                {
                    status = 0,
                    msg = "no user to be found"
                };

            DateTime validStartDate, validEndDate;
            if (DateTime.TryParse(startDate, out validStartDate))
            {
                userQuery = userQuery.Where(u => u.CreatedAt > validStartDate);
            }
            if (DateTime.TryParse(endDate, out validEndDate))
            {
                userQuery = userQuery.Where(u => u.CreatedAt < validEndDate);
            }
            if (!string.IsNullOrEmpty(searchTerm)) userQuery = userQuery.Where(u => u.Name.Contains(searchTerm) || u.Email.Contains(searchTerm) || u.Specialty!.Contains(searchTerm));

            if (!string.IsNullOrEmpty(sortColumn))
            {
                Expression<Func<User, object>> keySelector = sortColumn.ToLower() switch // throws error when sortColumn is null
                {
                    "name" or "n" => User => User.Name,
                    "email" or "e" => User => User.Email,
                    "country" or "co" => User => User.Country!,
                    "state" or "s" => User => User.State!,
                    "city" or "ci" => User => User.City!,
                    "height" or "h" => User => User.HeightCm!,
                    "weight" or "w" => User => User.WeightKg!,
                    _ => User => User.UserID,
                };
                if (!string.IsNullOrEmpty(OrderBy)) userQuery = userQuery.OrderBy(keySelector);
                else userQuery = userQuery.OrderBy(keySelector);
            }
            var userResponse = userQuery
                                .Select(u => new UserViewDTO
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
                                });
            var users = await PagedList<UserViewDTO>.CreateAsync(userResponse, page, pageSize);
            return new GettersResponse<UserViewDTO>
            {
                status = 2,
                msg = "Successfull",
                Data = users
            };
        }
        public async Task<GettersResponse<UserViewDTO>> GetAllUsers(int page, int pageSize)
        {
            var userQuery =   from u in _db.Users
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

            if (userQuery == null || userQuery.Count() == 0)
                return new GettersResponse<UserViewDTO>
                {
                    status = 0,
                    msg = "no user to be found"
                };

            var users = await PagedList<UserViewDTO>.CreateAsync(userQuery,page,pageSize);
            return new GettersResponse<UserViewDTO>
            {
                status = 2,
                msg = "Successfull",
                Data = users
            };
        }

    }
}
