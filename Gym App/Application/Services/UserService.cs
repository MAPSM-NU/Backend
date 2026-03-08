using Gym_App.Domain;
using Gym_App.Domain.Entities;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.Context;
using Gym_App.Infastructure.DTOs.UserDTOs;
using Gym_App.Infastructure.Interfaces.Repositries;
using Gym_App.Infastructure.Interfaces.Services;
using Gym_App.Infastructure.Transfer_Classes;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace Gym_App.Application.Services
{
    public class UserService : IUserServise
    {
        private readonly IUserRepositry _userRepositry;
        private readonly DbBase _db;
        private readonly ITokenHandler _tokenHandler;
        public UserService(IUserRepositry userRepositry, DbBase db, ITokenHandler tokenHandler)
        {
            _userRepositry = userRepositry;
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
                Id = Guid.NewGuid(),
                Name = u.Name,
                Email = u.Email,
                Password = new PasswordHasher<User>().HashPassword(null, u.Password),
                CreatedAt = DateTime.Now,
                UserType = "Admin",
                Role = role
            };

            //Generating Tokens
            var Token = await _tokenHandler.CreateAccessToken(user.Id, user.Name, user.Email, user.Role.RoleName);
            var RefreshToken = await _tokenHandler.CreateRefreshToken(user.Id);

            //Saving to Database via repository for user
            await _userRepositry.Create(user);

            // Save refresh token using Db (keeps refresh token storage separate)
            await _db.RefreshTokens.AddAsync(new RefreshTokens
            {
                UserID = user.Id,
                RefreshToken = RefreshToken,
                Expires = DateTime.Now.AddDays(4)
            });
            await _db.SaveChangesAsync();

            return new ResponseToken
            {
                Status = 1,
                AccessToken = Token,
                RefreshToken = RefreshToken,
                msg = "Admin created successfully"
            };
        }
        public async Task<ResponseToken> SignUpUser(UserCreationDTO u)//0 == missing Information. 1 == Name already in use. 2 == Email is in use. 3 == Email not valid. 4 == Password not valid. 
                                                                      //5 == Succesful signup
        {
            //Signing up the user
            if (u == null || u.Name == null || u.Email == null || u.Password == null)
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
                Id = Guid.NewGuid(),
                Name = u.Name,
                Email = u.Email,
                Password = new PasswordHasher<User>().HashPassword(null, u.Password),
                CreatedAt = DateTime.Now,
                Role = role
            };

            //Setting UserType to Coach/Doctor/Trainee
            if (!string.IsNullOrEmpty(u.UserType))
            {
                if (u.UserType!.ToLower() == "coach" || u.UserType.ToLower() == "c")
                    user.UserType = "Coach";
                else if (u.UserType.ToLower() == "doctor" || u.UserType.ToLower() == "d")
                    user.UserType = "Doctor";
                else
                    user.UserType = "Trainee";
            }
            else
                user.UserType = "Trainee";

            //Generating Tokens
            var Token = await _tokenHandler.CreateAccessToken(user.Id, user.Name, user.Email, user.Role.RoleName);
            var RefreshToken = await _tokenHandler.CreateRefreshToken(user.Id);

            //Saving to Database via repository for user
            await _userRepositry.Create(user);

            // Save refresh token
            await _db.RefreshTokens.AddAsync(new RefreshTokens
            {
                UserID = user.Id,
                RefreshToken = RefreshToken,
                Expires = DateTime.Now.AddDays(4)
            });
            await _db.SaveChangesAsync();

            return new ResponseToken
            {
                Status = 1,
                AccessToken = Token,
                RefreshToken = RefreshToken,
                msg = "User created successfully"
            };
        }
        public async Task<ResponseToken> SigninUser(string email, string password) // 0 ==  mail not found. 1 == password is wrong . 2 == succesful login
        {   //Checking if the user exists
            var isUserExists = await _db.Users.Include(u => u.Role)
                                      .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

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
                var AccessToken = await _tokenHandler.CreateAccessToken(isUserExists.Id, isUserExists.Name, isUserExists.Email, isUserExists.Role.RoleName);
                var RefreshToken = await _tokenHandler.RefreshingToken(isUserExists.Id);

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

            //Getting the user from repository
            var isUserExists = await _userRepositry.GetById(user.Id);
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
            if (user.DOB != default)
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

            //Saving to Database via repository
            await _userRepositry.Update(isUserExists);
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

            var user = await _userRepositry.GetById(User.Id);
            //If user not found return
            if (user is null)
                return new SettersResponse { status = 0, msg = "Invalid user data." };

            //Making all usertypes into lowercase letters
            var usertype = (user.UserType ?? string.Empty).ToLower();
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
                if (incomingUsertype == "coach" || incomingUsertype == "doctor")
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

            //Saving to Database via repository
            await _userRepositry.Update(user);
            return new SettersResponse { status = 1, msg = "User type changed successfully." };
        }
        public async Task<SettersResponse> DeleteUser(Guid Id)//0 == error || 1 == successfull
        {
            //Checking Id validity
            if (Id == Guid.Empty)
                return new SettersResponse { status = 0, msg = "Invalid user ID." };

            //Use repository to delete
            await _userRepositry.Delete(Id);
            return new SettersResponse { status = 1, msg = "User deleted successfully." };
        }

        //-----------------------------------------------------------------------


        //        *********** Extra Validation Function ***********

        public async Task<bool> isNameValid(string name)//checks if the name is already taken
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            // repository returns true if name exists; invert to match previous semantics
            var exists = await _userRepositry.isUserNameExist(name);
            return !exists;
        }
        public async Task<bool> EmailExists(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            var exists = await _userRepositry.isUserEmailExist(email);
            return exists;
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

        public async Task<GettersResponse<UserViewDTO>> GetUserByID(Guid Id)
        {
            if(Id == Guid.Empty)
                return new GettersResponse<UserViewDTO>
                {
                    status = 0,
                    msg = "Faulty ID"
                };

            var userEntity = await _userRepositry.GetById(Id);
            if (userEntity == null)
                return new GettersResponse<UserViewDTO>
                {
                    status = 0,
                    msg = "User not found"
                };

            var user = new UserViewDTO
            {
                Id = Id,
                Name = userEntity.Name,
                Email = userEntity.Email,
                Bio = userEntity.Bio,
                CreatedAt = userEntity.CreatedAt,
                DOB = userEntity.DOB,
                State = userEntity.State,
                City = userEntity.City,
                Country = userEntity.Country,
                PhoneNumber = userEntity.PhoneNumber,
                ProfilePictureUrl = userEntity.ProfilePictureUrl,
                subscriptionPlan = userEntity.subscriptionPlan,
                HeightCm = userEntity.HeightCm,
                WeightKg = userEntity.WeightKg,
                UserType = userEntity.UserType
            };

            return new GettersResponse<UserViewDTO>
            {
                status = 2,
                msg = "Successful",
                Value = user
            };
        }
        public async Task<GettersResponse<UserMiniViewDTO>> GetMiniUsers(string startDate, string endDate, int page, string sortColumn, string OrderBy, string searchTerm, int pageSize)
        {
            IQueryable<User> userQuery = _userRepositry.GetAll();

            if (userQuery == null || userQuery.Count() == 0)
                return new GettersResponse<UserMiniViewDTO>
                {
                    status = 0,
                    msg = "no user to be found"
                };

            DateTime validStartDate, validEndDate;
            if(DateTime.TryParse(startDate,out validStartDate) && DateTime.TryParse(endDate, out validEndDate)) 
                userQuery = _userRepositry.FilterDate(validStartDate, validEndDate,userQuery);
           
            if (!string.IsNullOrEmpty(searchTerm)) userQuery = _userRepositry.Search(searchTerm, userQuery);

            if (!string.IsNullOrEmpty(sortColumn)) userQuery = _userRepositry.FilterSortColumn(sortColumn, OrderBy, userQuery);
            
            var userResponse = userQuery
                                .Select(u => new UserMiniViewDTO
                                {
                                    Id = u.Id,
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
            IQueryable<User> userQuery = _userRepositry.GetAll();

            if (userQuery == null || userQuery.Count() == 0)
                return new GettersResponse<UserViewDTO>
                {
                    status = 0,
                    msg = "no user to be found"
                };

            DateTime validStartDate, validEndDate;
            if (DateTime.TryParse(startDate, out validStartDate) && DateTime.TryParse(endDate, out validEndDate))
            {
                userQuery = _userRepositry.FilterDate(validStartDate, validEndDate, userQuery);
            }
            if (!string.IsNullOrEmpty(searchTerm)) userQuery = _userRepositry.Search(searchTerm, userQuery);

            if (!string.IsNullOrEmpty(sortColumn))
            {
                userQuery = _userRepositry.FilterSortColumn(sortColumn, OrderBy, userQuery);
            }
            var userResponse = userQuery
                                .Select(u => new UserViewDTO
                                {
                                    Id = u.Id,
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
            var userQuery =   from u in _userRepositry.GetAll()
                              select new UserViewDTO
                               {
                                   Id = u.Id,
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
