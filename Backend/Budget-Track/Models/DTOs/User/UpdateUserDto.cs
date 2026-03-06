#nullable enable
using System.ComponentModel.DataAnnotations;

namespace Budget_Track.Models.DTOs.User
{
    public class UpdateUserDto
    {
        [Required]
        [StringLength(50, MinimumLength = 1)]
        public required string FirstName { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 1)]
        public required string LastName { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(100, MinimumLength = 6)]
        public string? Password { get; set; }

        [Required]
        public int RoleId { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
