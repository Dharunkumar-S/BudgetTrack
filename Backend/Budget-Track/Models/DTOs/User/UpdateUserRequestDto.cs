#nullable enable
using System.ComponentModel.DataAnnotations;
using Budget_Track.Models.Enums;

namespace Budget_Track.Models.DTOs.User
{
    public class UpdateUserRequestDto
    {
        [Required(ErrorMessage = "Employee ID is required")]
        [StringLength(50)]
        public required string EmployeeId { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, MinimumLength = 1)]
        public required string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, MinimumLength = 1)]
        public required string LastName { get; set; }

        [Required(ErrorMessage = "Role ID is required")]
        public int RoleId { get; set; }

        [Required(ErrorMessage = "Role Name is required")]
        [StringLength(100)]
        public required string RoleName { get; set; }

        [Required(ErrorMessage = "Department ID is required")]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Department Name is required")]
        [StringLength(100)]
        public required string DepartmentName { get; set; }

        public int? ManagerId { get; set; }

        [StringLength(100, MinimumLength = 1)]
        public string? ManagerName { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public UserStatus Status { get; set; }
    }
}
