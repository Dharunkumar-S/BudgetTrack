#nullable enable
using Budget_Track.Models.Enums;

namespace Budget_Track.Models.DTOs
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public required string EmployeeId { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public int DepartmentID { get; set; }
        public string? DepartmentName { get; set; }
        public int RoleID { get; set; }
        public string? RoleName { get; set; }
        public int? ManagerID { get; set; }
        public string? ManagerName { get; set; }
        public string? ManagerEmployeeId { get; set; }
        public UserStatus Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string FullName => $"{FirstName} {LastName}";
    }
}
