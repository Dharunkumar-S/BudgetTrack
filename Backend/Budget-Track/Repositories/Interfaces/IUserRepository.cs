using Budget_Track.Models.DTOs;
using Budget_Track.Models.DTOs.User;
using Budget_Track.Models.Entities;

namespace Budget_Track.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetUserForLoginAsync(string email);
        Task<User?> ValidateRefreshTokenAsync(int userId, string refreshToken);
        Task<User?> GetByEmployeeIdAsync(string employeeId);
        Task<User?> GetByIdWithManagerAsync(int id);
        Task<User?> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<User>> GetAllAsync();
        Task<IEnumerable<User>> GetAllManagersAsync();
        Task<IEnumerable<User>> GetEmployeesByManagerIdAsync(int managerId);
        Task<bool> UserExistsAsync(int id);
        Task<bool> EmailExistsAsync(string email);

        Task<string> GenerateEmployeeIdAsync(int roleId);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(int id, int deletedByUserId);
        Task SaveChangesAsync();

        // Stored Procedure Methods
        Task<UserProfileResponseDto?> GetUserProfileByIdAsync(int userId);
        Task<(List<UserListResponseDto> Users, int TotalCount)> GetUsersListAsync(
            int? roleId,
            string? search,
            int? departmentId,
            int? managerId,
            bool? isDeleted,
            bool? isActive,
            int pageNumber,
            int pageSize
        );
    }
}
