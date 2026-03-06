#nullable enable
using System.ComponentModel.DataAnnotations;

namespace Budget_Track.Models.DTOs
{
	public class UserLoginDto
	{
		[Required(ErrorMessage = "Email is required")]
		[EmailAddress(ErrorMessage = "Invalid email format")]
		public required string Email { get; set; }

		[Required(ErrorMessage = "Password is required")]
		[StringLength(255, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
		public required string Password { get; set; }
	}
}
