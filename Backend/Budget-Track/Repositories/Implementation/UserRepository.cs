using Budget_Track.Data;
using Budget_Track.Models.DTOs.User;
using Budget_Track.Models.Entities;
using Budget_Track.Models.Enums;
using Budget_Track.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Budget_Track.Repositories.Implementation
{
    public class UserRepository : IUserRepository
    {
        private readonly BudgetTrackDbContext _context;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(BudgetTrackDbContext context, ILogger<UserRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> GenerateEmployeeIdAsync(int roleId)
        {
            var prefix = roleId == 1 ? "ADM" : roleId == 2 ? "MGR" : "EMP";

            // Include soft-deleted users so we never reuse an EmployeeID that
            // already exists in the database.
            // Fetch all matching IDs and find the true numeric maximum in memory
            // to avoid lexicographic string ordering bugs (e.g. "EMP0009" > "EMP0010").
            var allIds = await _context.Users
                .IgnoreQueryFilters()
                .Where(u => u.EmployeeID.StartsWith(prefix))
                .Select(u => u.EmployeeID)
                .ToListAsync();

            int maxNum = 0;
            foreach (var id in allIds)
            {
                if (int.TryParse(id.Substring(prefix.Length), out int num))
                    maxNum = Math.Max(maxNum, num);
            }

            return $"{prefix}{maxNum + 1:D4}";
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            try
            {
                return await _context
                    .Users.Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.UserID == id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting user by id {id}: {ex.Message}");
                throw;
            }
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            try
            {
                return await _context
                    .Users.Include(u => u.Department)
                    .Include(u => u.Role)
                    .Include(u => u.Manager)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting user by email: {ex.Message}");
                throw;
            }
        }

        public async Task<User?> GetUserForLoginAsync(string email)
        {
            try
            {
                // Mirrors uspLoginCheck: only returns a user when Status = 1 (Active)
                // and the account is not soft-deleted.
                return await _context.Users
                    .Include(u => u.Department)
                    .Include(u => u.Role)
                    .Include(u => u.Manager)
                    .FirstOrDefaultAsync(u =>
                        u.Email.ToLower() == email.ToLower()
                        && !u.IsDeleted
                        && u.Status == UserStatus.Active);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting user for login ({email}): {ex.Message}");
                throw;
            }
        }

        public async Task<User?> ValidateRefreshTokenAsync(int userId, string refreshToken)
        {
            try
            {
                // Mirrors uspValidateRefreshToken: returns a user only when Status = 1 (Active),
                // the stored token matches, and the token has not expired.
                return await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u =>
                        u.UserID == userId
                        && u.RefreshToken == refreshToken
                        && u.RefreshTokenExpiryTime > DateTime.UtcNow
                        && !u.IsDeleted
                        && u.Status == UserStatus.Active);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error validating refresh token for userId {userId}: {ex.Message}");
                throw;
            }
        }

        public async Task<User?> GetByEmployeeIdAsync(string employeeId)
        {
            try
            {
                return await _context
                    .Users.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.EmployeeID == employeeId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting user by EmployeeID: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            try
            {
                return await _context.Users.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting all users: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<User>> GetAllManagersAsync()
        {
            try
            {
                return await _context
                    .Users.AsNoTracking()
                    .Where(u => (u.RoleID == 1 || u.RoleID == 2) && u.Status == UserStatus.Active)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting all managers: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<User>> GetEmployeesByManagerIdAsync(int managerId)
        {
            try
            {
                return await _context
                    .Users.AsNoTracking()
                    .Where(u =>
                        u.ManagerID == managerId && u.RoleID == 3 && u.Status == UserStatus.Active
                    )
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting employees by manager id: {ex.Message}");
                throw;
            }
        }

        public async Task<User?> GetByIdWithManagerAsync(int id)
        {
            try
            {
                return await _context
                    .Users.Include(u => u.Manager)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserID == id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting user with manager: {ex.Message}");
                throw;
            }
        }

        public async Task<User?> GetByIdWithDetailsAsync(int id)
        {
            try
            {
                return await _context
                    .Users.Include(u => u.Department)
                    .Include(u => u.Role)
                    .Include(u => u.Manager)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserID == id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting user with details: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> UserExistsAsync(int id)
        {
            try
            {
                return await _context.Users.AnyAsync(u => u.UserID == id);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking user existence: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            try
            {
                return await _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error checking email existence: {ex.Message}");
                throw;
            }
        }

        public async Task AddAsync(User user)
        {
            try
            {
                await _context.Users.AddAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding user: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateAsync(User user)
        {
            try
            {
                _context.Users.Update(user);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating user: {ex.Message}");
                throw;
            }
        }

        public async Task DeleteAsync(int id, int deletedByUserId)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user != null)
                {
                    user.Status = UserStatus.Inactive;
                    user.IsDeleted = true;
                    user.DeletedDate = DateTime.UtcNow;
                    user.DeletedByUserID = deletedByUserId;
                    user.UpdatedDate = DateTime.UtcNow;
                    user.UpdatedByUserID = deletedByUserId;

                    // Scramble Email and EmployeeID so the DB unique indexes
                    // (IX_tUser_Email, IX_tUser_EmployeeID) are freed up for reuse.
                    // The UserID is preserved in the scrambled value for audit purposes.
                    user.Email = $"deleted_{user.UserID}@removed.invalid";
                    user.EmployeeID = $"DEL{user.UserID:D6}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting user: {ex.Message}");
                throw;
            }
        }

        public async Task SaveChangesAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Database error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error saving changes: {ex.Message}");
                throw;
            }
        }

        public async Task<UserProfileResponseDto?> GetUserProfileByIdAsync(int userId)
        {
            try
            {
                return await _context
                    .Users.Include(u => u.Department)
                    .Include(u => u.Role)
                    .Include(u => u.Manager)
                    .AsNoTracking()
                    .Where(u => u.UserID == userId)
                    .Select(u => new UserProfileResponseDto
                    {
                        UserId = u.UserID,
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        Email = u.Email,
                        EmployeeId = u.EmployeeID,
                        DepartmentId = u.DepartmentID,
                        DepartmentName = u.Department.DepartmentName,
                        ManagerId = u.ManagerID,
                        ManagerEmployeeId = u.Manager != null ? u.Manager.EmployeeID : null,
                        ManagerName =
                            u.Manager != null
                                ? u.Manager.FirstName + " " + u.Manager.LastName
                                : null,
                        RoleId = u.RoleID,
                        RoleName = u.Role.RoleName,
                        Status = u.Status,
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting user profile for userId {userId}: {ex.Message}");
                throw;
            }
        }

        // Pure EF Core — paginated user list with filters
        public async Task<(List<UserListResponseDto> Users, int TotalCount)> GetUsersListAsync(
            int? roleId,
            string? search,
            int? departmentId,
            int? managerId,
            bool? isDeleted,
            bool? isActive,
            int pageNumber,
            int pageSize
        )
        {
            // IgnoreQueryFilters so we explicitly control IsDeleted visibility
            var query = _context.Users.IgnoreQueryFilters().AsNoTracking().AsQueryable();

            // --- Filters ---
            if (roleId.HasValue)
                query = query.Where(u => u.RoleID == roleId.Value);

            // Search by Employee ID or Name
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(u =>
                    u.EmployeeID.ToLower().Contains(term)
                    || (u.FirstName + " " + u.LastName).ToLower().Contains(term)
                );
            }

            if (departmentId.HasValue)
                query = query.Where(u => u.DepartmentID == departmentId.Value);

            if (managerId.HasValue)
                query = query.Where(u => u.ManagerID == managerId.Value);

            if (isDeleted.HasValue)
                query = query.Where(u => u.IsDeleted == isDeleted.Value);
            else
                query = query.Where(u => !u.IsDeleted); // default: non-deleted only

            if (isActive.HasValue)
                query = query.Where(u => (u.Status == UserStatus.Active) == isActive.Value);

            // Total count BEFORE pagination
            var totalCount = await query.CountAsync();

            // Paginated + projected to DTO in one SQL query
            var users = await query
                .OrderByDescending(u => u.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserListResponseDto
                {
                    UserId = u.UserID,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    EmployeeId = u.EmployeeID,
                    DepartmentId = u.DepartmentID,
                    DepartmentName = u.Department.DepartmentName,
                    ManagerId = u.ManagerID,
                    ManagerEmployeeId = u.Manager != null ? u.Manager.EmployeeID : null,
                    ManagerName =
                        u.Manager != null ? u.Manager.FirstName + " " + u.Manager.LastName : null,
                    RoleId = u.RoleID,
                    RoleName = u.Role.RoleName,
                    Status = u.Status,
                    IsActive = u.Status == UserStatus.Active,
                    IsDeleted = u.IsDeleted,
                    CreatedDate = u.CreatedDate,
                    UpdatedDate = u.UpdatedDate,
                })
                .ToListAsync();

            _logger.LogInformation(
                "GetUsersListAsync: page {Page}, {Count}/{Total} users",
                pageNumber,
                users.Count,
                totalCount
            );

            return (users, totalCount);
        }
    }
}
