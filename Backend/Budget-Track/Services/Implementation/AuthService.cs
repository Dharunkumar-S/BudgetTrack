using Budget_Track.Models.DTOs;
using Budget_Track.Models.DTOs.User;
using Budget_Track.Models.Entities;
using Budget_Track.Models.Enums;
using Budget_Track.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Budget_Track.Services.Implementation
{
    public interface IAuthService
    {
        /// <summary>
        /// Admin-only: Register a new employee or manager
        /// </summary>
        Task<AuthResponseDto> AdminRegisterUserAsync(AdminUserRegisterDto registerDto, int adminId);

        /// <summary>
        /// User login - available to all roles
        /// </summary>
        Task<AuthResponseDto> LoginAsync(UserLoginDto loginDto);

        /// <summary>
        /// Refresh access token
        /// </summary>
        Task<AuthResponseDto> RefreshTokenAsync(string accessToken, string refreshToken);

        Task<AuthResponseDto> ChangePasswordAsync(
            int userId,
            string oldPassword,
            string newPassword
        );

        /// <summary>
        /// Admin-only: Update user details including password
        /// </summary>
        Task<AuthResponseDto> UpdateUserAsync(
            int userId,
            UpdateUserByAdminDto updateDto,
            int adminId
        );

        /// <summary>
        /// Logout and revoke token
        /// </summary>
        Task<bool> RevokeTokenAsync(int userId);
    }

    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            IJwtTokenService jwtTokenService,
            IPasswordHasher<User> passwordHasher,
            ILogger<AuthService> logger
        )
        {
            _userRepository =
                userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _jwtTokenService =
                jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
            _passwordHasher =
                passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AuthResponseDto> AdminRegisterUserAsync(
            AdminUserRegisterDto registerDto,
            int adminId
        )
        {
            try
            {
                // 1) Ensure admin (RoleID 1 = Admin)
                var admin = await _userRepository.GetByIdAsync(adminId);
                if (admin == null || admin.RoleID != 1)
                {
                    _logger.LogWarning(
                        "Unauthorized registration attempt by userId: {AdminId}",
                        adminId
                    );
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Only Admin users can register new employees/managers",
                    };
                }

                // 2) Normalize inputs (single responsibility: normalize here; repo compares as-is)
                var email = registerDto.Email?.Trim();
                var firstName = registerDto.FirstName?.Trim();
                var lastName = registerDto.LastName?.Trim();
                var managerEmployeeId = registerDto.ManagerEmployeeId?.Trim();

                // 3) Basic validations
                if (registerDto.DepartmentID <= 0)
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Department is required",
                    };

                if (string.IsNullOrWhiteSpace(firstName))
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "First name is required",
                    };

                if (string.IsNullOrWhiteSpace(lastName))
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Last name is required",
                    };

                if (string.IsNullOrWhiteSpace(email))
                    return new AuthResponseDto { Success = false, Message = "Email is required" };

                if (string.IsNullOrWhiteSpace(registerDto.Password))
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Password is required",
                    };

                // 4) Uniqueness checks (avoid hitting unique index exception)
                var existingByEmail = await _userRepository.GetByEmailAsync(email);
                if (existingByEmail != null)
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Email already registered",
                    };

                // 5) Manager resolution (RoleID 3 = Employee, RoleID 2 = Manager, RoleID 1 = Admin)
                User? resolvedManager = null;
                int? managerIdToAssign = null;

                if (registerDto.RoleID > 1) // Only Managers (2) and Employees (3) can have managers
                {
                    if (!string.IsNullOrWhiteSpace(managerEmployeeId))
                    {
                        resolvedManager = await _userRepository.GetByEmployeeIdAsync(
                            managerEmployeeId
                        );
                        if (resolvedManager == null || resolvedManager.RoleID >= registerDto.RoleID)
                        {
                            _logger.LogWarning(
                                "Invalid manager assignment. Target role: {TargetRole}, ManagerEmployeeId: {ManagerEmployeeId}(Role:{ManagerRole})",
                                registerDto.RoleID,
                                managerEmployeeId,
                                resolvedManager?.RoleID
                            );
                            return new AuthResponseDto
                            {
                                Success = false,
                                Message =
                                    $"Invalid manager assignment: manager must have a higher role than the user.",
                            };
                        }
                        managerIdToAssign = resolvedManager.UserID;
                    }
                    else if (registerDto.RoleID == 3) // Requirement: Employees must have a manager
                    {
                        return new AuthResponseDto
                        {
                            Success = false,
                            Message = "Manager is required for Employee roles",
                        };
                    }
                }
                else
                {
                    // Admins must not have managers assigned
                    if (!string.IsNullOrWhiteSpace(managerEmployeeId))
                        return new AuthResponseDto
                        {
                            Success = false,
                            Message = "Admins cannot have a manager assigned",
                        };
                }

                // 6) Auto-generate EmployeeID based on role (ADM/MGR/EMP + 4-digit sequence)
                var employeeId = await _userRepository.GenerateEmployeeIdAsync(registerDto.RoleID);

                // 7) Create user
                var user = new User
                {
                    EmployeeID = employeeId,
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email!,
                    DepartmentID = registerDto.DepartmentID,
                    PasswordHash = "",
                    RoleID = registerDto.RoleID,
                    ManagerID = managerIdToAssign,
                    Status = UserStatus.Active,
                    CreatedDate = DateTime.UtcNow,
                    CreatedByUserID = adminId,
                };

                user.PasswordHash = _passwordHasher.HashPassword(user, registerDto.Password);

                await _userRepository.AddAsync(user);

                try
                {
                    await _userRepository.SaveChangesAsync();
                }
                catch (DbUpdateException dbEx)
                {
                    // Race-condition friendly messages for unique index violations
                    _logger.LogError(dbEx, "Unique constraint violation during admin registration");
                    var innerMsg = dbEx.InnerException?.Message ?? dbEx.Message;
                    if (innerMsg.Contains("IX_tUser_Email", StringComparison.OrdinalIgnoreCase)
                        || innerMsg.Contains("Email", StringComparison.OrdinalIgnoreCase))
                    {
                        return new AuthResponseDto { Success = false, Message = "Email already registered" };
                    }
                    if (innerMsg.Contains("IX_tUser_EmployeeID", StringComparison.OrdinalIgnoreCase)
                        || innerMsg.Contains("EmployeeID", StringComparison.OrdinalIgnoreCase))
                    {
                        return new AuthResponseDto { Success = false, Message = "Employee ID conflict. Please try again." };
                    }
                    return new AuthResponseDto { Success = false, Message = "Registration failed due to a data conflict. Please try again." };
                }

                // 8) Reload user with navigation properties (Department, Role, Manager)
                var createdUser = await _userRepository.GetByIdWithDetailsAsync(user.UserID);
                if (createdUser == null)
                {
                    _logger.LogError(
                        "Failed to reload created user with ID: {UserId}",
                        user.UserID
                    );
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "User created but failed to load details",
                    };
                }

                // 8) Build response DTO with all navigation properties loaded
                var dto = MapToUserResponseDto(createdUser);

                // Get role name for response message
                var roleName = registerDto.RoleID switch
                {
                    1 => "Admin",
                    2 => "Manager",
                    3 => "Employee",
                    _ => "User",
                };

                return new AuthResponseDto
                {
                    Success = true,
                    Message = $"User registered successfully as {roleName}",
                    User = dto,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during admin registration");
                return new AuthResponseDto { Success = false, Message = "Registration failed" };
            }
        }

        public async Task<AuthResponseDto> ChangePasswordAsync(
            int userId,
            string oldPassword,
            string newPassword
        )
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return new AuthResponseDto { Success = false, Message = "User not found" };

                // verify old (temporary) password
                var verify = _passwordHasher.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    oldPassword
                );
                if (verify == PasswordVerificationResult.Failed)
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Old password is incorrect",
                    };

                // set new password
                user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
                user.UpdatedDate = DateTime.UtcNow;
                user.UpdatedByUserID = userId;

                // optional: clear refresh token so they must re-auth after change
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;

                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();

                _logger.LogInformation(
                    "Password changed successfully for user {Email}",
                    user.Email
                );

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Password changed successfully",
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for userId {UserId}", userId);
                return new AuthResponseDto { Success = false, Message = "Password change failed" };
            }
        }

        public async Task<AuthResponseDto> UpdateUserAsync(
            int userId,
            UpdateUserByAdminDto updateDto,
            int adminId
        )
        {
            try
            {
                // 1) Ensure admin (RoleID 1 = Admin)
                var admin = await _userRepository.GetByIdAsync(adminId);
                if (admin == null || admin.RoleID != 1)
                {
                    _logger.LogWarning("Unauthorized update attempt by userId: {AdminId}", adminId);
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Only Admin users can update user details",
                    };
                }

                // 2) Get existing user
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return new AuthResponseDto { Success = false, Message = "User not found" };
                }

                // 3) Normalize inputs
                var email = updateDto.Email?.Trim();
                var firstName = updateDto.FirstName?.Trim();
                var lastName = updateDto.LastName?.Trim();
                var managerEmployeeId = updateDto.ManagerEmployeeId?.Trim();

                // 4) Basic validations
                if (updateDto.DepartmentID <= 0)
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Department is required",
                    };

                if (string.IsNullOrWhiteSpace(firstName))
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "First name is required",
                    };

                if (string.IsNullOrWhiteSpace(lastName))
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Last name is required",
                    };

                if (string.IsNullOrWhiteSpace(email))
                    return new AuthResponseDto { Success = false, Message = "Email is required" };

                // 5) Uniqueness checks (if email or employeeId changed)
                if (!user.Email.Equals(email, StringComparison.OrdinalIgnoreCase))
                {
                    var existingByEmail = await _userRepository.GetByEmailAsync(email);
                    if (existingByEmail != null)
                        return new AuthResponseDto
                        {
                            Success = false,
                            Message = "Email already registered",
                        };
                }

                // 6) Manager resolution (RoleID 3 = Employee, RoleID 2 = Manager, RoleID 1 = Admin)
                User? resolvedManager = null;
                int? managerIdToAssign = null;

                if (updateDto.RoleID > 1) // Only Managers (2) and Employees (3) can have managers
                {
                    if (!string.IsNullOrWhiteSpace(managerEmployeeId))
                    {
                        resolvedManager = await _userRepository.GetByEmployeeIdAsync(
                            managerEmployeeId
                        );
                        if (resolvedManager == null || resolvedManager.RoleID >= updateDto.RoleID)
                        {
                            _logger.LogWarning(
                                "Invalid manager assignment. Target role: {TargetRole}, ManagerEmployeeId: {ManagerEmployeeId}(Role:{ManagerRole})",
                                updateDto.RoleID,
                                managerEmployeeId,
                                resolvedManager?.RoleID
                            );
                            return new AuthResponseDto
                            {
                                Success = false,
                                Message =
                                    $"Invalid manager assignment: manager must have a higher role than the user.",
                            };
                        }
                        managerIdToAssign = resolvedManager.UserID;
                    }
                    else if (updateDto.RoleID == 3) // Requirement: Employees must have a manager
                    {
                        return new AuthResponseDto
                        {
                            Success = false,
                            Message = "Manager is required for Employee roles",
                        };
                    }
                }
                else
                {
                    // Admins must not have managers assigned
                    if (!string.IsNullOrWhiteSpace(managerEmployeeId))
                        return new AuthResponseDto
                        {
                            Success = false,
                            Message = "Admins cannot have a manager assigned",
                        };
                }

                // 7) Update user properties
                user.FirstName = firstName;
                user.LastName = lastName;
                user.Email = email!;
                user.DepartmentID = updateDto.DepartmentID;
                user.RoleID = updateDto.RoleID;
                user.ManagerID = managerIdToAssign;
                user.Status = updateDto.Status;
                user.UpdatedDate = DateTime.UtcNow;
                user.UpdatedByUserID = adminId;

                // 8) Update password if provided
                if (!string.IsNullOrWhiteSpace(updateDto.Password))
                {
                    user.PasswordHash = _passwordHasher.HashPassword(user, updateDto.Password);
                    // Clear refresh token so user must re-login with new password
                    user.RefreshToken = null;
                    user.RefreshTokenExpiryTime = null;
                }

                await _userRepository.UpdateAsync(user);

                try
                {
                    await _userRepository.SaveChangesAsync();
                }
                catch (DbUpdateException dbEx)
                {
                    _logger.LogError(dbEx, "Error updating user during save");
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Failed to update user",
                    };
                }

                // 9) Reload user with navigation properties
                var updatedUser = await _userRepository.GetByIdWithDetailsAsync(user.UserID);
                if (updatedUser == null)
                {
                    _logger.LogError(
                        "Failed to reload updated user with ID: {UserId}",
                        user.UserID
                    );
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "User updated but failed to load details",
                    };
                }

                // 10) Build response DTO
                var dto = MapToUserResponseDto(updatedUser);

                var roleName = updateDto.RoleID switch
                {
                    1 => "Admin",
                    2 => "Manager",
                    3 => "Employee",
                    _ => "User",
                };

                return new AuthResponseDto
                {
                    Success = true,
                    Message = $"User updated successfully as {roleName}",
                    User = dto,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", userId);
                return new AuthResponseDto { Success = false, Message = "Update failed" };
            }
        }

        private string GenerateTemporaryPassword()
        {
            const string chars =
                "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%";
            var random = new Random();
            var password = new char[12];
            for (int i = 0; i < password.Length; i++)
            {
                password[i] = chars[random.Next(chars.Length)];
            }
            return new string(password);
        }

        public async Task<AuthResponseDto> LoginAsync(UserLoginDto loginDto)
        {
            try
            {
                // Find user by email — SP enforces Status = 1 (Active) at the database level
                var user = await _userRepository.GetUserForLoginAsync(loginDto.Email);
                if (user == null || user.Status != UserStatus.Active)
                {
                    _logger.LogWarning($"Login attempt with invalid credentials: {loginDto.Email}");
                    return new AuthResponseDto { Success = false, Message = "Invalid credentials" };
                }

                // Verify password
                var result = _passwordHasher.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    loginDto.Password
                );
                if (result == PasswordVerificationResult.Failed)
                {
                    _logger.LogWarning($"Failed login attempt: {loginDto.Email}");
                    return new AuthResponseDto { Success = false, Message = "Invalid credentials" };
                }

                // Update last login
                user.LastLoginDate = DateTime.UtcNow;

                // Generate tokens
                var accessToken = _jwtTokenService.GenerateAccessToken(user);
                var refreshToken = _jwtTokenService.GenerateRefreshToken();

                // Save refresh token
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();

                _logger.LogInformation($"User logged in successfully: {user.Email}");

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Login successful",
                    User = MapToUserResponseDto(user),
                    Token = new TokenDto
                    {
                        AccessToken = accessToken.AccessToken,
                        RefreshToken = refreshToken,
                        ExpiresAt = accessToken.ExpiresAt,
                        ExpiresIn = accessToken.ExpiresIn,
                    },
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during login: {ex.Message}");
                return new AuthResponseDto { Success = false, Message = "Login failed" };
            }
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(
            string accessToken,
            string refreshToken
        )
        {
            try
            {
                var principal = _jwtTokenService.GetPrincipalFromExpiredToken(accessToken);
                var userIdClaim = principal?.FindFirst(
                    System.Security.Claims.ClaimTypes.NameIdentifier
                );

                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return new AuthResponseDto { Success = false, Message = "Invalid token" };
                }

                // uspValidateRefreshToken enforces Status = 1, token match, and token expiry.
                // Returns null if the user is inactive, the token is wrong, or it has expired.
                var user = await _userRepository.ValidateRefreshTokenAsync(userId, refreshToken);
                if (user == null)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Invalid refresh token",
                    };
                }

                var newAccessToken = _jwtTokenService.GenerateAccessToken(user);
                var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

                user.RefreshToken = newRefreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Token refreshed successfully",
                    Token = new TokenDto
                    {
                        AccessToken = newAccessToken.AccessToken,
                        RefreshToken = newRefreshToken,
                        ExpiresAt = newAccessToken.ExpiresAt,
                        ExpiresIn = newAccessToken.ExpiresIn,
                    },
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error refreshing token: {ex.Message}");
                return new AuthResponseDto { Success = false, Message = "Token refresh failed" };
            }
        }

        public async Task<bool> RevokeTokenAsync(int userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return false;

                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
                await _userRepository.UpdateAsync(user);
                await _userRepository.SaveChangesAsync();

                _logger.LogInformation($"Token revoked for user: {user.Email}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error revoking token: {ex.Message}");
                return false;
            }
        }

        private UserResponseDto MapToUserResponseDto(User user)
        {
            return new UserResponseDto
            {
                Id = user.UserID,
                EmployeeId = user.EmployeeID,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                DepartmentID = user.DepartmentID,
                DepartmentName = user.Department?.DepartmentName,
                RoleID = user.RoleID,
                RoleName = user.Role?.RoleName,
                ManagerID = user.ManagerID,
                ManagerName =
                    user.Manager != null
                        ? user.Manager.FirstName + " " + user.Manager.LastName
                        : null,
                ManagerEmployeeId = user.Manager != null ? user.Manager.EmployeeID : null,
                Status = user.Status,
                CreatedDate = user.CreatedDate,
                UpdatedDate = user.UpdatedDate,
            };
        }
    }
}
