#nullable enable
using System.ComponentModel.DataAnnotations;
using Budget_Track.Models.Enums;

namespace Budget_Track.Models.DTOs
{
    public class ChangePasswordDto
    {
        [Required]
        public required string OldPassword { get; set; }

        [Required]
        [StringLength(
            100,
            MinimumLength = 8,
            ErrorMessage = "New password must be at least 8 characters long."
        )]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character (@$!%*?&)."
        )]
        public required string NewPassword { get; set; }
    }

    public class AdminUserRegisterDto
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

        [Required(ErrorMessage = "Password is required")]
        [StringLength(
            100,
            MinimumLength = 8,
            ErrorMessage = "Password must be at least 8 characters long"
        )]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character (@$!%*?&)."
        )]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Role is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid role")]
        public int RoleID { get; set; }

        [Required(ErrorMessage = "Department is required")]
        public int DepartmentID { get; set; }

        [StringLength(50)]
        public string? ManagerEmployeeId { get; set; }
    }
}
