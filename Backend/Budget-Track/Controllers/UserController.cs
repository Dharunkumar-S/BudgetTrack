using System.Security.Claims;
using Budget_Track.Data;
using Budget_Track.Models.DTOs;
using Budget_Track.Models.Entities;
using Budget_Track.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Budget_Track.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly BudgetTrackDbContext _context;
        private readonly ILogger<UserController> _logger;

        public UserController(
            IUserRepository userRepository,
            BudgetTrackDbContext context,
            ILogger<UserController> logger
        )
        {
            _userRepository =
                userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Note: GET api/users (list) is in AuthController
        // Note: GET api/users/profile is in AuthController
        // Note: PUT api/users/{userId} is in AuthController

        [HttpGet("stats")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserStats()
        {
            try
            {
                // Global query filter already excludes IsDeleted users
                var totalUsers = await _context.Users.CountAsync();
                var activeUsers = await _context.Users.CountAsync(u =>
                    u.Status == Budget_Track.Models.Enums.UserStatus.Active
                );
                var inactiveUsers = totalUsers - activeUsers;

                var roleCounts = await _context
                    .Users.GroupBy(u => u.RoleID)
                    .Select(g => new { RoleId = g.Key, Count = g.Count() })
                    .ToListAsync();

                return Ok(
                    new
                    {
                        totalUsers,
                        admins = roleCounts.FirstOrDefault(r => r.RoleId == 1)?.Count ?? 0,
                        managers = roleCounts.FirstOrDefault(r => r.RoleId == 2)?.Count ?? 0,
                        employees = roleCounts.FirstOrDefault(r => r.RoleId == 3)?.Count ?? 0,
                        activeUsers,
                        inactiveUsers,
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching user stats: {ex.Message}");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "Failed to get user stats" }
                );
            }
        }

        [HttpGet("managers")]
        [Authorize]
        public async Task<IActionResult> GetAllManagers()
        {
            try
            {
                var managers = await _userRepository.GetAllManagersAsync();
                var managerDtos = managers
                    .Select(m => new
                    {
                        employeeId = m.EmployeeID,
                        firstName = m.FirstName,
                        lastName = m.LastName,
                        email = m.Email,
                        fullName = $"{m.FirstName} {m.LastName}",
                        roleId = m.RoleID,
                        departmentId = m.DepartmentID,
                    })
                    .ToList();
                return Ok(managerDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting all managers: {ex.Message}");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "Failed to get managers" }
                );
            }
        }

        [HttpGet("{managerUserId:int}/employees")]
        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<IActionResult> GetEmployeesByManager(int managerUserId)
        {
            try
            {
                var role = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value;
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (
                    !string.IsNullOrWhiteSpace(role)
                    && (
                        role.Equals("Manager", StringComparison.OrdinalIgnoreCase)
                        || role.Equals("Employee", StringComparison.OrdinalIgnoreCase)
                    )
                )
                {
                    if (
                        string.IsNullOrWhiteSpace(userIdClaim)
                        || !int.TryParse(userIdClaim, out var currentUserId)
                    )
                        return Unauthorized(new { message = "User not authenticated" });

                    // Check access based on role
                    if (role.Equals("Manager", StringComparison.OrdinalIgnoreCase))
                    {
                        if (currentUserId != managerUserId)
                            return Forbid();
                    }
                    else if (role.Equals("Employee", StringComparison.OrdinalIgnoreCase))
                    {
                        var me = await _userRepository.GetByIdAsync(currentUserId);
                        if (me == null || me.ManagerID != managerUserId)
                            return Forbid();
                    }
                }

                var manager = await _userRepository.GetByIdAsync(managerUserId);
                if (manager == null || manager.RoleID != 2)
                    return NotFound(new { message = "Manager not found" });

                var employees = await _userRepository.GetEmployeesByManagerIdAsync(managerUserId);
                var sameDepartmentEmployees = employees.Where(e =>
                    e.DepartmentID == manager.DepartmentID
                );
                var employeeDtos = sameDepartmentEmployees.Select(MapToUserResponseDto).ToList();
                return Ok(employeeDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting employees by manager: {ex.Message}");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "Failed to get employees" }
                );
            }
        }

        // DELETE api/users/{userId}
        [HttpDelete("{userId:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            try
            {
                var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(adminIdClaim, out int adminId))
                    return Unauthorized(new { message = "User not authenticated" });

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return NotFound(new { message = "User not found" });

                await _userRepository.DeleteAsync(userId, adminId);
                await _userRepository.SaveChangesAsync();

                _logger.LogInformation($"User with UserID {userId} soft-deleted by admin {adminId}");
                return Ok(new { success = true, message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting user: {ex.Message}");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { message = "Failed to delete user" }
                );
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
                        ? $"{user.Manager.FirstName} {user.Manager.LastName}"
                        : null,
                ManagerEmployeeId = user.Manager != null ? user.Manager.EmployeeID : null,
                Status = user.Status,
                CreatedDate = user.CreatedDate,
                UpdatedDate = user.UpdatedDate,
            };
        }
    }
}