#nullable enable
using System.ComponentModel.DataAnnotations;

namespace Budget_Track.Models.DTOs.User
{
    public class CreateUserRequestDto
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(
            50,
            MinimumLength = 1,
            ErrorMessage = "First name must be between 1 and 50 characters"
        )]
        public required string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(
            50,
            MinimumLength = 1,
            ErrorMessage = "Last name must be between 1 and 50 characters"
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
        public int RoleId { get; set; }

        [Required(ErrorMessage = "Department is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid department")]
        public int DepartmentId { get; set; }

        // Only required when RoleId corresponds to Employee
        public string? ManagerEmployeeId { get; set; }
    }
}
