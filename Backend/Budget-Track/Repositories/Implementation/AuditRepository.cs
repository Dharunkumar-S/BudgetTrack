using System.Text.Json;
using Budget_Track.Data;
using Budget_Track.Models.DTOs.Audit;
using Budget_Track.Models.DTOs.Pagination;
using Budget_Track.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Budget_Track.Repositories.Implementation
{
    public class AuditRepository : IAuditRepository
    {
        private readonly BudgetTrackDbContext _context;

        public AuditRepository(BudgetTrackDbContext context)
        {
            _context = context;
        }

        public async Task<List<AuditLogDto>> GetAllAuditLogsAsync()
        {
            var auditLogs = await _context
                .AuditLogs.Include(a => a.User)
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();

            return auditLogs
                .Select(a => new AuditLogDto
                {
                    AuditLogID = a.AuditLogID,
                    UserID = a.UserID,
                    UserName =
                        a.User != null ? a.User.FirstName + " " + a.User.LastName : "Unknown",
                    EmployeeId = a.User?.EmployeeID,
                    EntityType = a.EntityType,
                    EntityID = a.EntityID,
                    Action = a.Action.ToString(),
                    OldValue = ParseJsonValue(a.OldValue),
                    NewValue = ParseJsonValue(a.NewValue),
                    Timestamp = a.CreatedDate,
                    Notes = a.Description,
                })
                .ToList();
        }

        public async Task<PagedResult<AuditLogDto>> GetAllAuditLogsPaginatedAsync(
            int pageNumber,
            int pageSize,
            string? search = null,
            string? action = null,
            string? entityType = null
        )
        {
            var query = _context.AuditLogs.Include(a => a.User).AsQueryable();

            // Search by Employee ID or Employee Name
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(a =>
                    (a.User != null && a.User.EmployeeID.ToLower().Contains(term))
                    || (
                        a.User != null
                        && (a.User.FirstName + " " + a.User.LastName).ToLower().Contains(term)
                    )
                );
            }

            // Filter by action — parse the string to the AuditAction enum
            if (
                !string.IsNullOrWhiteSpace(action)
                && Enum.TryParse<Models.Enums.AuditAction>(
                    action,
                    ignoreCase: true,
                    out var parsedAction
                )
            )
            {
                query = query.Where(a => a.Action == parsedAction);
            }

            // Filter by module (entityType)
            if (!string.IsNullOrWhiteSpace(entityType))
                query = query.Where(a => a.EntityType == entityType);

            var totalRecords = await query.CountAsync();

            var auditLogs = await query
                .OrderByDescending(a => a.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var auditLogDtos = auditLogs
                .Select(a => new AuditLogDto
                {
                    AuditLogID = a.AuditLogID,
                    UserID = a.UserID,
                    UserName =
                        a.User != null ? a.User.FirstName + " " + a.User.LastName : "Unknown",
                    EmployeeId = a.User?.EmployeeID,
                    EntityType = a.EntityType,
                    EntityID = a.EntityID,
                    Action = a.Action.ToString(),
                    OldValue = ParseJsonValue(a.OldValue),
                    NewValue = ParseJsonValue(a.NewValue),
                    Timestamp = a.CreatedDate,
                    Notes = a.Description,
                })
                .ToList();

            return PagedResult<AuditLogDto>.Create(
                auditLogDtos,
                pageNumber,
                pageSize,
                totalRecords
            );
        }

        public async Task<List<AuditLogDto>> GetAuditLogsByUserIdAsync(int userId)
        {
            var auditLogs = await _context
                .AuditLogs.Include(a => a.User)
                .Where(a => a.UserID == (int?)userId)
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();

            return auditLogs
                .Select(a => new AuditLogDto
                {
                    AuditLogID = a.AuditLogID,
                    UserID = a.UserID,
                    UserName =
                        a.User != null ? a.User.FirstName + " " + a.User.LastName : "Unknown",
                    EmployeeId = a.User?.EmployeeID,
                    EntityType = a.EntityType,
                    EntityID = a.EntityID,
                    Action = a.Action.ToString(),
                    OldValue = ParseJsonValue(a.OldValue),
                    NewValue = ParseJsonValue(a.NewValue),
                    Timestamp = a.CreatedDate,
                    Notes = a.Description,
                })
                .ToList();
        }

        private static object? ParseJsonValue(string? jsonString)
        {
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<object>(jsonString);
            }
            catch
            {
                return jsonString;
            }
        }
    }
}
