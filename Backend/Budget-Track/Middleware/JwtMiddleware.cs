using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Budget_Track.Models.Enums;
using Budget_Track.Repositories.Interfaces;

namespace Budget_Track.Middleware
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JwtMiddleware> _logger;

        public JwtMiddleware(RequestDelegate next, ILogger<JwtMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context, JwtSettings jwtSettings, IUserRepository userRepository)
        {
            var token = ExtractTokenFromHeader(context);

            if (!string.IsNullOrWhiteSpace(token))
            {
                await AttachUserToContext(context, token!, jwtSettings, userRepository);
            }

            await _next(context);
        }

        private static string? ExtractTokenFromHeader(HttpContext context)
        {
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(authHeader))
                return null;

            const string prefix = "Bearer ";
            if (authHeader.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return authHeader.Substring(prefix.Length).Trim();
            }
            return null;
        }

        private async Task AttachUserToContext(HttpContext context, string token, JwtSettings jwtSettings, IUserRepository userRepository)
        {
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey));
                var tokenHandler = new JwtSecurityTokenHandler();

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;

                // Numeric DB user id
                var idClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                int userId;
                var hasUserId = int.TryParse(idClaim, out userId);

                // String EmployeeId (optional)
                var employeeId = jwtToken.Claims.FirstOrDefault(c => c.Type == "EmployeeId")?.Value;

                if (hasUserId)
                {
                    var user = await userRepository.GetByIdAsync(userId);
                    if (user != null && user.Status == UserStatus.Active)
                    {
                        context.Items["User"] = user;
                        context.Items["UserId"] = userId; // int
                        if (!string.IsNullOrWhiteSpace(employeeId))
                        {
                            context.Items["EmployeeId"] = employeeId; // string
                        }
                        _logger.LogInformation("User {UserId} authenticated via JWT", userId);
                    }
                }
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning("Invalid token: {Message}", ex.Message);
                // Token is invalid/expired — do not attach user, let the pipeline continue.
                // [Authorize] endpoints will return 401; [AllowAnonymous] endpoints (e.g. logout) will proceed normally.
            }
            catch (Exception ex)
            {
                _logger.LogError("Token validation error: {Message}", ex.Message);
                // Non-security errors — same: skip user attachment, continue pipeline.
            }
        }
    }
}