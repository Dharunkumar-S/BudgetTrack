#nullable enable
namespace Budget_Track.Models.DTOs
{
	public class AuthResponseDto
	{
		public bool Success { get; set; }
		public string Message { get; set; } = null!;
		public UserResponseDto? User { get; set; }
		public TokenDto? Token { get; set; }
	}

	public class TokenDto
	{
		public string AccessToken { get; set; } = null!;
		public string RefreshToken { get; set; } = null!;
		public DateTime ExpiresAt { get; set; }
		public int ExpiresIn { get; set; }
		public string TokenType { get; set; } = "Bearer";
	}
}
