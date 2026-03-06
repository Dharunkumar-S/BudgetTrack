#nullable enable
using System.ComponentModel.DataAnnotations;
using Budget_Track.Models.Enums;

namespace Budget_Track.Models.DTOs.User
{
    /// <summary>
    /// DTO for admin to update user details including password
    /// </summary>
    public class UpdateUserByAdminDto
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(
            50,
            MinimumLength = 1,
            ErrorMessage = "First name cannot be more than 50 characters"
        )]
        public required string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(
            50,
            MinimumLength = 1,
            ErrorMessage = "Last name cannot be more than 50 characters"
        )]
        public required string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100)]
        public required string Email { get; set; }

        /// <summary>
        /// Optional: If provided, password will be updated
        /// </summary>
        [StringLength(
            100,
            MinimumLength = 8,
            ErrorMessage = "Password must be at least 8 characters"
        )]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Role is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid role")]
        public int RoleID { get; set; }

        [Required(ErrorMessage = "Department is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid department")]
        public int DepartmentID { get; set; }

        [StringLength(50)]
        public string? ManagerEmployeeId { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public UserStatus Status { get; set; }
    }
}
