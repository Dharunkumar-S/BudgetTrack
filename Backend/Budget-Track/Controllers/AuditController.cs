using Budget_Track.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Budget_Track.Controllers
{
    [ApiController]
    [Route("api/audits")]
    public class AuditController : ControllerBase
    {
        private readonly IAuditService _auditService;

        public AuditController(IAuditService auditService)
        {
            _auditService = auditService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllAudit(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null,
            [FromQuery] string? action = null,
            [FromQuery] string? entityType = null
        )
        {
            try
            {
                var auditLogs = await _auditService.GetAllAuditLogsPaginatedAsync(
                    pageNumber,
                    pageSize,
                    search,
                    action,
                    entityType
                );
                return Ok(auditLogs);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Failed to retrieve audit logs" });
            }
        }

        [HttpGet("{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllAuditByUserId(int userId)
        {
            try
            {
                var auditLogs = await _auditService.GetAuditLogsByUserIdAsync(userId);

                if (auditLogs == null || auditLogs.Count == 0)
                {
                    return Ok(new List<object>());
                }

                return Ok(auditLogs);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Failed to retrieve audit logs" });
            }
        }
    }
}
