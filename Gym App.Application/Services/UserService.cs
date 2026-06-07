using Gym_App.Application.Authorization;
using Gym_App.Core;
using Gym_App.Domain;
using Gym_App.Domain.Entities;
using Gym_App.Domain.Transfer_Classes;
using Gym_App.Infastructure.DTOs.UserDTOs;
using Gym_App.Infastructure.Interfaces.Repositries;
using Gym_App.Infastructure.Interfaces.Services;
using Gym_App.Infastructure.Transfer_Classes;
using Gym_App.Infrastructure.Interfaces.Services;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Threading;

namespace Gym_App.Application.Services;

public class UserService : IUserServise
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenHandler _tokenHandler;
    private readonly IFileService _fileService;
    private readonly IEmailSender _emailSender;
    private readonly ICachedAuthorizationService _authorizationService;
    private readonly ILogger<UserService> _logger;
    public UserService(IUnitOfWork unitOfWork, ITokenHandler tokenHandler, IFileService fileService, ILogger<UserService> logger, ICachedAuthorizationService authorizationService, IEmailSender emailSender)
    {
        _unitOfWork = unitOfWork;
        _tokenHandler = tokenHandler;
        _fileService = fileService;
        _logger = logger;
        _emailSender = emailSender;
        _authorizationService = authorizationService;
    }

    //0 == Error(Bad Request) || 1 == Unauthorized (Forbid) || 2 == Success (Ok)

    public async Task<ResponseToken> CreateAdmin(UserCreationDTO u, CancellationToken cancellationToken = default)
    {

        if (u == null || u.Name == null || u.Email == null || u.Password == null)
            return new ResponseToken { Status = 0, msg = "Invalid Data" };

        try
        {
            if (!await isNameValid(u.Name, cancellationToken))
                return new ResponseToken { Status = 0, msg = "Name is already used" };

            if (!IsEmailValid(u.Email))
                return new ResponseToken { Status = 0, msg = "Invalid Email" };

            if (await _unitOfWork.Users.isUserEmailExist(u.Email, cancellationToken))
                return new ResponseToken { Status = 0, msg = "Email already in use" };

            if (!await IsPasswordValid(u.Password))
                return new ResponseToken { Status = 0, msg = "Invalid Password" };

            var role = await _unitOfWork.Roles.GetRoleByName("Admin", cancellationToken);
            if (role == null)
                return new ResponseToken { Status = 0, msg = "Role not found" };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = u.Name,
                Email = u.Email,
                Password = new Microsoft.AspNetCore.Identity.PasswordHasher<User>().HashPassword(null, u.Password),
                CreatedAt = DateTime.Now,
                UserType = "Admin",
                RoleID = role.Id,
                Role = role
            };

            var token = await _tokenHandler.CreateAccessToken(user.Id, user.Name, user.Email, role.RoleName, cancellationToken);
            var refreshToken = await _tokenHandler.CreateRefreshToken(user.Id, cancellationToken);

            await _unitOfWork.Users.Create(user, cancellationToken);

            var refreshTokenEntity = new RefreshTokens
            {
                UserID = user.Id,
                RefreshToken = refreshToken,
                Expires = DateTime.Now.AddDays(4)
            };
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new ResponseToken
            {
                Status = 2,
                AccessToken = token,
                RefreshToken = refreshToken,
                msg = "Admin created successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating an admin. Email: {Email}, Name: {Name}", u?.Email, u?.Name);
            return new ResponseToken { Status = 0, msg = "An error occurred while creating the admin." };
        }
    }

    public async Task<ResponseToken> SignUpUser(UserCreationDTO u, CancellationToken cancellationToken = default)
    {
        if (u == null || string.IsNullOrEmpty(u.Name) || string.IsNullOrEmpty(u.Email) || string.IsNullOrEmpty(u.Password))
        {
            _logger.LogWarning("User creation failed due to missing information. Name: {Name}, Email: {Email}", u?.Name, u?.Email);
            return new ResponseToken { Status = 0, msg = "Missing Information" };
        }
        try
        {
            if (!await isNameValid(u.Name, cancellationToken))
            {
                _logger.LogWarning("User creation failed due to name already in use. Name: {Name}", u.Name);
                return new ResponseToken { Status = 0, msg = "Name is already used" };
            }

            if (!IsEmailValid(u.Email))
            {
                _logger.LogWarning("User creation failed due to invalid email format. Email: {Email}", u.Email);
                return new ResponseToken { Status = 0, msg = "Invalid Email" };
            }

            if (await _unitOfWork.Users.isUserEmailExist(u.Email, cancellationToken))
            {
                _logger.LogWarning("User creation failed due to email already in use. Email: {Email}", u.Email);
                return new ResponseToken { Status = 0, msg = "Email already in use" };
            }

            if (!await IsPasswordValid(u.Password))
            {
                _logger.LogWarning("User creation failed due to invalid password. Email: {Email}", u.Email);
                return new ResponseToken { Status = 0, msg = "Invalid Password" };
            }

            var role = await _unitOfWork.Roles.GetRoleByName("User", cancellationToken);
            if (role == null)
            {
                _logger.LogWarning("User creation failed due to role not found. Role: {Role}", "User");
                return new ResponseToken { Status = 0, msg = "Role not found" };
            }


            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = u.Name,
                Email = u.Email,
                Password = new PasswordHasher<User>().HashPassword(null, u.Password),
                CreatedAt = DateTime.Now,
                RoleID = role.Id,
                Role = role,
            };

            if (!string.IsNullOrEmpty(u.UserType))
            {
                user.UserType = u.UserType.ToLower() switch
                {
                    "coach" or "c" => "Coach",
                    "doctor" or "d" => "Doctor",
                    _ => "Trainee"
                };
            }
            else
            {
                user.UserType = "Trainee";
            }

            var token = await _tokenHandler.CreateAccessToken(user.Id, user.Name, user.Email, role.RoleName, cancellationToken);
            var refreshToken = await _tokenHandler.CreateRefreshToken(user.Id, cancellationToken);

            await _unitOfWork.Users.Create(user, cancellationToken);

            var refreshTokenEntity = new RefreshTokens
            {
                UserID = user.Id,
                RefreshToken = refreshToken,
                Expires = DateTime.Now.AddDays(4)
            };
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation($"User created with email: {u.Email} and name: {u.Name}");
            await _emailSender.IntroductionEmail(u.Email, cancellationToken);
            return new ResponseToken
            {
                Status = 2,
                AccessToken = token,
                RefreshToken = refreshToken,
                msg = "User created successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while creating a user. Email: {u?.Email}, Name: {u?.Name}");
            return new ResponseToken { Status = 0, msg = "An error occurred while creating the user." };
        }
    }

    public async Task<ResponseToken> SigninUser(string email, string password, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetUserByEmail(email, true, cancellationToken);

        if (user == null)
            return new ResponseToken { Status = 0, msg = "User not found" };

        var result = new PasswordHasher<User>().VerifyHashedPassword(user, user.Password, password);
        if (result == Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed)
            return new ResponseToken { Status = 0, msg = "Invalid Password" };

        var accessToken = await _tokenHandler.CreateAccessToken(user.Id, user.Name, user.Email, user.Role.RoleName, cancellationToken);
        var refreshToken = await _tokenHandler.RefreshingToken(user.Id, cancellationToken);

        return new ResponseToken
        {
            Status = 2,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            msg = "Login successful"
        };
    }
    public async Task<SettersResponse> ForgotPassword(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _unitOfWork.Users.GetUserByEmail(email, true, cancellationToken);
            if (user == null)
            {
                _logger.LogError($"Trying to create forget password token for non existing email: {email}");
                return new SettersResponse { status = 0, msg = "User not found" };
            }
            var otpCode = GenerateOtpCode();
            var existingToken = await _unitOfWork.PasswordResetToken.GetTokenByUserId(user.Id, cancellationToken);
            if (existingToken != null)
            {
                existingToken.OTP = otpCode;
                existingToken.Expiration = DateTime.Now.AddMinutes(15);
                existingToken.isUsed = false;
                await _unitOfWork.PasswordResetToken.Update(existingToken, cancellationToken);

            }
            else
            {
                var resetToken = new PasswordResetToken
                {
                    userId = user.Id,
                    OTP = otpCode,
                    Expiration = DateTime.Now.AddMinutes(15),
                    Email = email,
                    UpdatedAt = DateTime.Now,
                    CreatedAt = DateTime.Now,
                    User = user

                };
                await _unitOfWork.PasswordResetToken.Create(resetToken, cancellationToken);
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation($"Password reset OTP generated for email: {email}");
            string body = $"Your OTP code for password reset is: {otpCode}. This code will expire in 15 minutes.";
            await _emailSender.SendEmailAsync(email, "Password Reset OTP", body, cancellationToken);
            return new SettersResponse { status = 2, msg = "Password reset OTP sent successfully" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while processing forgot password for email: {email}");
            return new SettersResponse { status = 0, msg = "An error occurred while processing the request." };
        }
    }
    public async Task<SettersResponse> ResetPassword(string email, string otp, string newPassword, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _unitOfWork.Users.GetUserByEmail(email, true, cancellationToken);
            if (user == null)
            {
                _logger.LogError($"Password reset failed: User not found for email: {email}");
                return new SettersResponse { status = 0, msg = "User not found" };
            }

            var verificationResult = await VerifyOTP(email, otp, cancellationToken);
            if (verificationResult.status != 2)
            {
                _logger.LogWarning($"Password reset failed during OTP verification for email: {email}. Reason: {verificationResult.msg}");
                return new SettersResponse { status = 0, msg = "OTP verification failed: " + verificationResult.msg };
            }

            if (!await IsPasswordValid(newPassword))
            {
                _logger.LogWarning($"Password reset failed due to invalid new password for email: {email}");
                return new SettersResponse { status = 0, msg = "Invalid new password" };
            }

            if (new PasswordHasher<User>().VerifyHashedPassword(user, user.Password, newPassword) == Microsoft.AspNetCore.Identity.PasswordVerificationResult.Success)
            {
                _logger.LogWarning($"User inputed right otp {otp} yet the new password is the same as the old password for email: {email}");
                return new SettersResponse { status = 0, msg = "New password cannot be the same as the old password" };
            }
            user.Password = new PasswordHasher<User>().HashPassword(user, newPassword);

            await _unitOfWork.Users.Update(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation($"Password reset successfully for email: {email}");
            return new SettersResponse { status = 2, msg = "Password reset successfully" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while resetting password for email: {email}");
            return new SettersResponse { status = 0, msg = "An error occurred while processing the request." };
        }
    }
    public async Task<SettersResponse> UpdateUser(UserUpdateDTO user, CancellationToken cancellationToken = default)
    {
        if (user == null)
        {
            _logger.LogWarning("UpdateUser failed due to null user data.");
            return new SettersResponse { status = 0, msg = "Invalid user data" };
        }
        try
        {
            var existingUser = await _unitOfWork.Users.GetById(user.Id, cancellationToken);
            if (existingUser is null)
            {
                _logger.LogWarning("UpdateUser failed because user was not found. UserID: {UserID}", user.Id);
                return new SettersResponse { status = 0, msg = "User not found" };
            }

            var authResult = await _authorizationService.IsUserAsync(user.Id);
            if (!authResult)
            {
                _logger.LogWarning("UpdateUser failed due to unauthorized access. UserID: {UserID}", user.Id);
                return new SettersResponse { status = 1, msg = "Unauthorized" };
            }


            if (!string.IsNullOrEmpty(user.Name))
            {
                if (!await isNameValid(user.Name, cancellationToken))
                    return new SettersResponse { status = 0, msg = "Name is not valid" };
                existingUser.Name = user.Name;
            }

            if (!string.IsNullOrEmpty(user.Bio))
                existingUser.Bio = user.Bio;
            if (user.DOB != default)
                existingUser.DOB = user.DOB;
            if (!string.IsNullOrEmpty(user.State))
                existingUser.State = user.State;
            if (!string.IsNullOrEmpty(user.City))
                existingUser.City = user.City;
            if (!string.IsNullOrEmpty(user.Country))
                existingUser.Country = user.Country;
            if (!string.IsNullOrEmpty(user.PhoneNumber))
                existingUser.PhoneNumber = user.PhoneNumber;
            if (user.HeightCm > 0)
                existingUser.HeightCm = user.HeightCm;
            if (user.WeightKg > 0)
                existingUser.WeightKg = user.WeightKg;

            if (user.ProfilePicture != null)
            {
                _logger.LogInformation($"Saving pfp for user email: {existingUser.Email}\n name : {existingUser.Name}");

                if (!string.IsNullOrEmpty(existingUser.ProfilePictureUrl))
                {
                    _logger.LogInformation($"Deleting old profile picture for user email: {existingUser.Email}\n name : {existingUser.Name}");
                    await _fileService.DeleteFileAsync(existingUser.ProfilePictureUrl, cancellationToken);
                }
                var imageResponse = await _fileService.UploadFileAsync(user.ProfilePicture!, new[] { ".jpg", ".png", ".jpeg" }, cancellationToken);
                if (imageResponse.status == 0)
                {
                    _logger.LogWarning("User creation failed due to profile picture upload error. Error: {Error}", imageResponse.msg);
                    return new SettersResponse { status = 0, msg = "Profile picture upload failed: " + imageResponse.msg };
                }
                else
                {
                    _logger.LogInformation($"New profile picture uploaded successfully for user email: {existingUser.Email}\n name : {existingUser.Name}");
                    string profilePictureUrl = imageResponse.msg;
                    existingUser.ProfilePictureUrl = profilePictureUrl;
                }
            }

            await _unitOfWork.Users.Update(existingUser, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation($"User updated successfully. UserID: {existingUser.Id}, Email: {existingUser.Email}");
            return new SettersResponse { status = 2, msg = "User updated successfully." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating the user. UserID: {UserID}", user.Id);
            return new SettersResponse { status = 0, msg = "An error occurred while updating the user." };
        }
    }
    public async Task<SettersResponse> ChangePfp(Guid userID, IFormFile pfp, CancellationToken cancellationToken = default)
    {
        if (userID == Guid.Empty || pfp == null)
        {
            _logger.LogError("ChangePfp failed due to invalid input. UserID: {UserID}, PFP: {PFP}", userID, pfp != null ? pfp.FileName : "null");
            return new SettersResponse { status = 0, msg = "Invalid user ID or profile picture" };
        }
        try
        {
            var user = await _unitOfWork.Users.GetById(userID, cancellationToken);
            if (user is null)
            {
                _logger.LogError("ChangePfp failed because user was not found. UserID: {UserID}", userID);
                return new SettersResponse { status = 0, msg = "User not found" };
            }

            var authResult = await _authorizationService.IsUserAsync(userID);
            if (!authResult)
            {
                _logger.LogWarning("ChangePfp failed due to unauthorized access. UserID: {UserID}", userID);
                return new SettersResponse { status = 1, msg = "Unauthorized" };
            }

            if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
            {
                _logger.LogInformation($"Deleting old profile picture for user email: {user.Email}\n name : {user.Name}");
                await _fileService.DeleteFileAsync(user.ProfilePictureUrl, cancellationToken);
            }

            var imageResponse = await _fileService.UploadFileAsync(pfp, new[] { ".jpg", ".png", ".jpeg" }, cancellationToken);
            if (imageResponse.status == 0)
            {
                _logger.LogError("ChangePfp failed due to profile picture upload error. UserID: {UserID}, Error: {Error}", userID, imageResponse.msg);
                return new SettersResponse { status = 0, msg = "Profile picture upload failed: " + imageResponse.msg };
            }
            user.ProfilePictureUrl = imageResponse.msg;
            await _unitOfWork.Users.Update(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation($"New Pfp set successfullt for User : {user.Email}");
            return new SettersResponse { status = 2, msg = "Profile picture updated successfully." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while changing the profile picture. UserID: {UserID}", userID);
            return new SettersResponse { status = 0, msg = "An error occurred while changing the profile picture." };
        }
    }
    public async Task<SettersResponse> DeletePfp(Guid userId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            _logger.LogError("DeletePfp failed due to invalid user ID. UserID: {UserID}", userId);
            return new SettersResponse { status = 0, msg = "Invalid user ID" };
        }
        try
        {
            var user = await _unitOfWork.Users.GetById(userId, cancellationToken);
            if (user is null)
            {
                _logger.LogError("DeletePfp failed because user was not found. UserID: {UserID}", userId);
                return new SettersResponse { status = 0, msg = "User not found" };
            }
            var authResult = await _authorizationService.IsUserAsync(userId);
            if (!authResult)
            {
                _logger.LogWarning("DeletePfp failed due to unauthorized access. UserID: {UserID}", userId);
                return new SettersResponse { status = 1, msg = "Unauthorized" };
            }
            if (string.IsNullOrEmpty(user.ProfilePictureUrl))
            {
                _logger.LogWarning("DeletePfp failed because user does not have a profile picture. UserID: {UserID}", userId);
                return new SettersResponse { status = 0, msg = "No profile picture to delete" };
            }
            await _fileService.DeleteFileAsync(user.ProfilePictureUrl, cancellationToken);
            user.ProfilePictureUrl = null;
            await _unitOfWork.Users.Update(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogInformation($"Profile picture deleted successfully for User : {user.Email}");
            return new SettersResponse { status = 2, msg = "Profile picture deleted successfully." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting the profile picture. UserID: {UserID}", userId);
            return new SettersResponse { status = 0, msg = "An error occurred while deleting the profile picture." };
        }
    }
    public async Task<SettersResponse> ChangeUserType(UserChangeTypeDTO userDto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (userDto == null || userDto.UserType == null)
                return new SettersResponse { status = 0, msg = "Invalid user type." };

            string keywords = "coach, c, doctor, d, trainee, t";
            if (!keywords.Contains(userDto.UserType.ToLower()))
                return new SettersResponse { status = 0, msg = "Invalid user type." };

            var user = await _unitOfWork.Users.GetById(userDto.Id, cancellationToken);
            if (user is null)
                return new SettersResponse { status = 0, msg = "Invalid user data." };

            var newUserType = userDto.UserType.ToLower() switch
            {
                "t" => "trainee",
                "c" => "coach",
                "d" => "doctor",
                _ => userDto.UserType.ToLower()
            };

            if ((user.UserType ?? "").ToLower() == newUserType)
                return new SettersResponse { status = 0, msg = "Same user type" };

            user.UserType = newUserType;
            if (newUserType == "coach" || newUserType == "doctor")
            {
                user.Specialty = userDto.Specialty;
                user.ExperienceYears = userDto.ExperienceYears;
                user.Certifications = userDto.Certifications;
            }
            else
            {
                user.Specialty = null;
                user.ExperienceYears = null;
                user.Certifications = null;
            }

            await _unitOfWork.Users.Update(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SettersResponse { status = 2, msg = "User type changed successfully." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while changing the user type. UserID: {UserID}", userDto?.Id);
            return new SettersResponse { status = 0, msg = "An error occurred while changing the user type." };
        }
    }

    public async Task<SettersResponse> DeleteUser(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            return new SettersResponse { status = 0, msg = "Invalid user ID" };

        try
        {
            var user = await _unitOfWork.Users.GetById(id, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("DeleteUser failed because user was not found. UserID: {UserID}", id);
                return new SettersResponse { status = 0, msg = "User not found" };
            }

            var authResult = await _authorizationService.IsUserAsync(id);
            if (!authResult)
            {
                _logger.LogWarning("DeleteUser failed due to unauthorized access. UserID: {UserID}", id);
                return new SettersResponse { status = 1, msg = "Unauthorized" };
            }

            await _unitOfWork.Users.Delete(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new SettersResponse { status = 2, msg = "User deleted successfully." };
        }
        catch (Exception ex)
        {
            return new SettersResponse { status = 0, msg = "User not found" };
        }
    }

    // ========== Getters ==========

    public async Task<GettersResponse<UserViewDTO>> GetUserByID(Guid id, CancellationToken cancellationToken = default)
    {
        if (id == Guid.Empty)
            return new GettersResponse<UserViewDTO>
            {
                status = 0,
                msg = "Faulty ID"
            };

        try
        {
            var userEntity = await _unitOfWork.Users.GetById(id, cancellationToken);
            if (userEntity == null)
                return new GettersResponse<UserViewDTO>
                {
                    status = 0,
                    msg = "User not found"
                };

            var user = new UserViewDTO
            {
                Id = id,
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving the user by ID. UserID: {UserID}", id);
            return new GettersResponse<UserViewDTO>
            {
                status = 0,
                msg = "An error occurred while retrieving the user."
            };
        }
    }

    public async Task<GettersResponse<UserMiniViewDTO>> GetMiniUsers(
        string startDate,
        string endDate,
        int page,
        string sortColumn,
        string OrderBy,
        string searchTerm,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userQuery = _unitOfWork.Users.GetAll();

            if (!userQuery.Any())
                return new GettersResponse<UserMiniViewDTO>
                {
                    status = 0,
                    msg = "no user to be found"
                };

            DateTime validStartDate, validEndDate;
            if (DateTime.TryParse(startDate, out validStartDate) && DateTime.TryParse(endDate, out validEndDate))
                userQuery = _unitOfWork.Users.FilterDate(validStartDate, validEndDate, userQuery);

            if (!string.IsNullOrEmpty(searchTerm))
                userQuery = _unitOfWork.Users.Search(searchTerm, userQuery);

            if (!string.IsNullOrEmpty(sortColumn))
                userQuery = _unitOfWork.Users.FilterSortColumn(sortColumn, OrderBy, userQuery);

            var userResponse = userQuery
                .Select(u => new UserMiniViewDTO
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    ProfilePictureUrl = u.ProfilePictureUrl
                });

            var users = await PagedList<UserMiniViewDTO>.CreateAsync(userResponse, page, pageSize, cancellationToken);

            return new GettersResponse<UserMiniViewDTO>
            {
                status = 2,
                msg = "Successfull",
                Data = users
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving mini user data. StartDate: {StartDate}, EndDate: {EndDate}, Page: {Page}, SortColumn: {SortColumn}, OrderBy: {OrderBy}, SearchTerm: {SearchTerm}, PageSize: {PageSize}", startDate, endDate, page, sortColumn, OrderBy, searchTerm, pageSize);
            return new GettersResponse<UserMiniViewDTO>
            {
                status = 0,
                msg = "An error occurred while retrieving users."
            };
        }
    }

    public async Task<GettersResponse<UserViewDTO>> GetUsers(
        string startDate,
        string endDate,
        int page,
        string sortColumn,
        string OrderBy,
        string searchTerm,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userQuery = _unitOfWork.Users.GetAll();

            if (!userQuery.Any())
                return new GettersResponse<UserViewDTO>
                {
                    status = 0,
                    msg = "no user to be found"
                };

            DateTime validStartDate, validEndDate;
            if (DateTime.TryParse(startDate, out validStartDate) && DateTime.TryParse(endDate, out validEndDate))
            {
                userQuery = _unitOfWork.Users.FilterDate(validStartDate, validEndDate, userQuery);
            }

            if (!string.IsNullOrEmpty(searchTerm))
                userQuery = _unitOfWork.Users.Search(searchTerm, userQuery);

            if (!string.IsNullOrEmpty(sortColumn))
            {
                userQuery = _unitOfWork.Users.FilterSortColumn(sortColumn, OrderBy, userQuery);
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

            var users = await PagedList<UserViewDTO>.CreateAsync(userResponse, page, pageSize, cancellationToken);

            return new GettersResponse<UserViewDTO>
            {
                status = 2,
                msg = "Successfull",
                Data = users
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving user data. StartDate: {StartDate}, EndDate: {EndDate}, Page: {Page}, SortColumn: {SortColumn}, OrderBy: {OrderBy}, SearchTerm: {SearchTerm}, PageSize: {PageSize}", startDate, endDate, page, sortColumn, OrderBy, searchTerm, pageSize);
            return new GettersResponse<UserViewDTO>
            {
                status = 0,
                msg = "An error occurred while retrieving users."
            };
        }
    }

    public async Task<GettersResponse<UserViewDTO>> GetAllUsers(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        try
        {
            var userQuery = from u in _unitOfWork.Users.GetAll()
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

            if (!userQuery.Any())
                return new GettersResponse<UserViewDTO>
                {
                    status = 0,
                    msg = "no user to be found"
                };

            var users = await PagedList<UserViewDTO>.CreateAsync(userQuery, page, pageSize, cancellationToken);

            return new GettersResponse<UserViewDTO>
            {
                status = 2,
                msg = "Successfull",
                Data = users
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving all users. Page: {Page}, PageSize: {PageSize}", page, pageSize);
            return new GettersResponse<UserViewDTO>
            {
                status = 0,
                msg = "An error occurred while retrieving users."
            };
        }
    }

    // ========== Helper Methods ==========

    private async Task<bool> isNameValid(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        var exists = await _unitOfWork.Users.isUserNameExist(name, cancellationToken);
        return !exists;
    }

    private bool IsEmailValid(string email)
    {
        return new EmailAddressAttribute().IsValid(email) && email != null;
    }

    private async Task<bool> IsPasswordValid(string password)
    {
        var PasswordPolicy = new PasswordValidator
        {
            RequiredLength = 8,
            RequireNonLetterOrDigit = false,
            RequireDigit = true,
            RequireLowercase = true,
            RequireUppercase = true,
        };

        var result = await PasswordPolicy.ValidateAsync(password);
        return result.Succeeded;
    }
    private string GenerateOtpCode()
    {
        Random random = new Random();
        int otp = random.Next(100000, 999999);
        return otp.ToString();
    }
    private async Task<SettersResponse> VerifyOTP(string email, string otp, CancellationToken cancellationToken = default)
    {
        try
        {
            var resetToken = await _unitOfWork.PasswordResetToken.GetTokenByUserEmail(email, cancellationToken);
            if (resetToken == null)
            {
                _logger.LogWarning($"OTP verification failed: No reset token found for email: {email}");
                return new SettersResponse { status = 0, msg = "Invalid OTP or email" };
            }
            if (resetToken.isUsed)
            {
                _logger.LogWarning($"OTP verification failed: OTP already used for email: {email}");
                return new SettersResponse { status = 0, msg = "OTP has already been used" };
            }
            if (resetToken.Expiration < DateTime.Now)
            {
                _logger.LogWarning($"OTP verification failed: OTP expired for email: {email}");
                return new SettersResponse { status = 0, msg = "OTP has expired" };
            }
            if (resetToken.OTP != otp)
            {
                _logger.LogWarning($"OTP verification failed: Invalid OTP provided for email: {email}");
                return new SettersResponse { status = 0, msg = "Invalid OTP" };
            }
            resetToken.isUsed = true;
            await _unitOfWork.PasswordResetToken.Update(resetToken, cancellationToken);
            return new SettersResponse { status = 2, msg = "OTP verified successfully" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while verifying OTP for email: {email}");
            return new SettersResponse { status = 0, msg = "An error occurred while processing the request." };
        }
    }

}
