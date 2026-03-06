using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Budget_Track.Data;
using Budget_Track.Models.DTOs;
using Budget_Track.Models.DTOs.Pagination;
using Budget_Track.Models.DTOs.User;
using Budget_Track.Models.Entities;
using Budget_Track.Models.Enums;
using Budget_Track.Repositories.Interfaces;
using Budget_Track.Services.Implementation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Budget_Track.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
        private readonly IUserRepository _userRepository;

        public AuthController(
            IAuthService authService,
            ILogger<AuthController> logger,
            IUserRepository userRepository
        )
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userRepository =
                userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        /// <summary>
        /// Admin-only: Register a new employee or manager
        /// Employees cannot self-register. Only Admin can register new users.
        /// </summary>
        [HttpPost("createuser")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminRegister([FromBody] AdminUserRegisterDto registerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Use numeric DB user id from the token
                var idValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrWhiteSpace(idValue) || !int.TryParse(idValue, out var adminId))
                    return Unauthorized(new { message = "User not authenticated" });

                var result = await _authService.AdminRegisterUserAsync(registerDto, adminId);

                if (!result.Success)
                    return BadRequest(result);

                _logger.LogInformation(
                    "New user registered by admin {AdminId}: {Email}",
                    adminId,
                    registerDto.Email
                );
                return StatusCode(StatusCodes.Status201Created, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration error");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "Registration failed" }
                );
            }
        }

        /// <summary>
        /// Login user
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] UserLoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _authService.LoginAsync(loginDto);

                if (!result.Success)
                {
                    return Unauthorized(result);
                }

                _logger.LogInformation($"User logged in: {loginDto.Email}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Login error: {ex.Message}");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "Login failed" }
                );
            }
        }

        [HttpPost("changepassword")]
        [Authorize] // user must be logged in with the temp password first
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var idValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrWhiteSpace(idValue) || !int.TryParse(idValue, out var userId))
                    return Unauthorized(new { message = "User not authenticated" });

                var result = await _authService.ChangePasswordAsync(
                    userId,
                    dto.OldPassword,
                    dto.NewPassword
                );

                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "Password change failed" }
                );
            }
        }

        /// <summary>
        /// Refresh access token
        /// </summary>
        [HttpPost("token/refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _authService.RefreshTokenAsync(
                    refreshTokenDto.AccessToken,
                    refreshTokenDto.RefreshToken
                );

                if (!result.Success)
                {
                    return Unauthorized(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Token refresh error: {ex.Message}");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "Token refresh failed" }
                );
            }
        }

        /// <summary>
        /// Logout user (revoke token)
        /// </summary>
        [HttpPost("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int?;
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                var result = await _authService.RevokeTokenAsync(userId.Value);
                if (!result)
                {
                    return BadRequest(new { message = "Logout failed" });
                }

                _logger.LogInformation($"User logged out: {userId}");
                return Ok(new { message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Logout error: {ex.Message}");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "Logout failed" }
                );
            }
        }

        /// <summary>
        /// Verify if user is authenticated
        /// </summary>
        [HttpGet("verify")]
        [Authorize]
        public IActionResult Verify()
        {
            try
            {
                var userId = HttpContext.Items["UserId"] as int?;
                if (userId == null)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }

                return Ok(new { message = "Token is valid", userId = userId.Value });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Verification error: {ex.Message}");
                return Unauthorized(new { message = "Invalid token" });
            }
        }

        /// <summary>
        /// GET api/users/profile
        /// Retrieves the authenticated user's profile information, including their Manager's details, based on the JWT claims
        /// Accessible by: Admin, Manager, Employee
        /// </summary>
        [HttpGet("/api/users/profile")]
        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<IActionResult> GetUserProfile()
        {
            try
            {
                var idValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrWhiteSpace(idValue) || !int.TryParse(idValue, out var userId))
                    return Unauthorized(new { message = "User not authenticated" });

                var userProfile = await _userRepository.GetUserProfileByIdAsync(userId);

                if (userProfile == null)
                    return NotFound(new { message = "User profile not found" });

                _logger.LogInformation("User profile retrieved: {UserId}", userId);
                return Ok(new { success = true, data = userProfile });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user profile");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { success = false, message = "Failed to retrieve user profile" }
                );
            }
        }

        /// <summary>
        /// GET api/users
        /// Retrieves a paginated list of all users (including deleted records)
        /// Supports filtering by Role, search (Employee ID or Name), isDeleted, and isActive with sorting
        /// Accessible by: Admin and Manager (Managers can only view direct-report employees)
        /// </summary>
        [HttpGet("/api/users")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetUsers(
            [FromQuery] int? roleId,
            [FromQuery] string? search,
            [FromQuery] int? departmentId,
            [FromQuery] bool? isDeleted,
            [FromQuery] bool? isActive,
            [FromQuery] string sortBy = "CreatedDate",
            [FromQuery] string sortOrder = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10
        )
        {
            try
            {
                var idValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (
                    string.IsNullOrWhiteSpace(idValue)
                    || !int.TryParse(idValue, out var callerUserId)
                )
                    return Unauthorized(new { message = "User not authenticated" });

                var roleValue =
                    User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value;
                var isManager = string.Equals(
                    roleValue,
                    "Manager",
                    StringComparison.OrdinalIgnoreCase
                );

                // Managers can only see employees directly reporting to them.
                int? managerId = isManager ? callerUserId : null;
                if (isManager)
                {
                    roleId = 3;
                }

                var (users, totalCount) = await _userRepository.GetUsersListAsync(
                    roleId,
                    search,
                    departmentId,
                    managerId,
                    isDeleted,
                    isActive,
                    pageNumber,
                    pageSize
                );

                var result = PagedResult<UserListResponseDto>.Create(
                    users,
                    pageNumber,
                    pageSize,
                    totalCount
                );

                _logger.LogInformation("Users list retrieved: {Count} users", users.Count);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users list");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { success = false, message = "Failed to retrieve users list" }
                );
            }
        }

        /// <summary>
        /// PUT api/users/{userId}
        /// Admin updates user details including optional password update
        /// Accessible by: Admin only
        /// </summary>
        [HttpPut("/api/users/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(
            [FromRoute] int userId,
            [FromBody] UpdateUserByAdminDto updateDto
        )
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // Get admin ID from token
                var idValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrWhiteSpace(idValue) || !int.TryParse(idValue, out var adminId))
                    return Unauthorized(new { message = "User not authenticated" });

                var result = await _authService.UpdateUserAsync(userId, updateDto, adminId);

                if (!result.Success)
                    return BadRequest(result);

                _logger.LogInformation("User {UserId} updated by admin {AdminId}", userId, adminId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", userId);
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { success = false, message = "Failed to update user" }
                );
            }
        }
    }

    public class RefreshTokenDto
    {
        [Required(ErrorMessage = "Access token is required")]
        public required string AccessToken { get; set; }

        [Required(ErrorMessage = "Refresh token is required")]
        public required string RefreshToken { get; set; }
    }
}
