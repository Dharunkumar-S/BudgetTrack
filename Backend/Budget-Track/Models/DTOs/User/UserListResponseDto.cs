#nullable enable
using Budget_Track.Models.Enums;

namespace Budget_Track.Models.DTOs.User
{
    public class UserListResponseDto
    {
        public int UserId { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string EmployeeId { get; set; }
        public int DepartmentId { get; set; }
        public required string DepartmentName { get; set; }
        public int? ManagerId { get; set; }
        public string? ManagerEmployeeId { get; set; }
        public string? ManagerName { get; set; }
        public int RoleId { get; set; }
        public required string RoleName { get; set; }
        public UserStatus Status { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string FullName => $"{FirstName} {LastName}";
    }
}
