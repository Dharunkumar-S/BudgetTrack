using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Budget_Track.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public abstract class BaseApiController : ControllerBase
    {
        /// <summary>
        /// Gets the authenticated user ID from JWT claims
        /// </summary>s
        protected int UserId
        {
            get
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    throw new UnauthorizedAccessException("User not authenticated");
                }
                return userId;
            }
        }

        /// <summary>
        /// Gets the authenticated user ID or throws UnauthorizedAccessException
        /// </summary>
        protected int GetUserId()
        {
            return UserId;
        }
    }
}