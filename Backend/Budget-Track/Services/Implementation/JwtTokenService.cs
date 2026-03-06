using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Budget_Track.Middleware;
using Budget_Track.Models.DTOs;
using Budget_Track.Models.Entities;

namespace Budget_Track.Services
{
    public interface IJwtTokenService
    {
        TokenDto GenerateAccessToken(User user);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }

    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<JwtTokenService> _logger;

        public JwtTokenService(JwtSettings jwtSettings, ILogger<JwtTokenService> logger)
        {
            _jwtSettings = jwtSettings ?? throw new ArgumentNullException(nameof(jwtSettings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public TokenDto GenerateAccessToken(User user)
        {
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var now = DateTime.UtcNow;
                var expiresAt = now.AddMinutes(_jwtSettings.ExpirationMinutes);

                var claims = new List<Claim>
                {
					// Server-side identity: numeric DB PK
					new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),

					// HR-style employee code (string) for FE/display
					new Claim("EmployeeId", user.EmployeeID),

					// Useful identity info
					new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.GivenName, user.FirstName),
                    new Claim(ClaimTypes.Surname, user.LastName),

					// Authorization - role name used for [Authorize(Roles = "...")] matching
					new Claim(ClaimTypes.Role, user.Role?.RoleName ?? string.Empty),
                    new Claim("RoleID", user.RoleID.ToString()),
                    new Claim("RoleName", user.Role?.RoleName ?? "Unknown"),

					// Status flags
					new Claim("UserStatus", user.Status.ToString()),

					// Token metadata
					new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                    new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                    new Claim(JwtRegisteredClaimNames.Nbf, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
                };

                // Department ID — needed for manager's budget create form auto-fill
                claims.Add(new Claim("DepartmentId", user.DepartmentID.ToString()));

                // Optional manager claim (only if present)
                if (user.ManagerID.HasValue)
                {
                    claims.Add(new Claim("ManagerId", user.ManagerID.Value.ToString()));
                }

                var token = new JwtSecurityToken(
                    issuer: _jwtSettings.Issuer,
                    audience: _jwtSettings.Audience,
                    claims: claims,
                    notBefore: now,
                    expires: expiresAt,
                    signingCredentials: creds
                );

                var tokenHandler = new JwtSecurityTokenHandler();
                var accessToken = tokenHandler.WriteToken(token);

                _logger.LogInformation("Access token generated for userId={UserID} email={Email}", user.UserID, user.Email);

                return new TokenDto
                {
                    AccessToken = accessToken,
                    ExpiresAt = expiresAt,
                    ExpiresIn = _jwtSettings.ExpirationMinutes * 60,
                    TokenType = "Bearer"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating access token for userId={UserID}", user.UserID);
                throw;
            }
        }


        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateLifetime = false // Ignore expiration for refresh token validation
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

                if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SecurityTokenException("Invalid token");
                }

                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error validating token: {ex.Message}");
                throw;
            }
        }
    }
}
