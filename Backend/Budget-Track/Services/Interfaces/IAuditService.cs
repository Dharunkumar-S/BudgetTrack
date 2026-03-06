using Budget_Track.Models.DTOs.Audit;
using Budget_Track.Models.DTOs.Pagination;

namespace Budget_Track.Services.Interfaces
{
    public interface IAuditService
    {
        Task<List<AuditLogDto>> GetAllAuditLogsAsync();
        Task<PagedResult<AuditLogDto>> GetAllAuditLogsPaginatedAsync(
            int pageNumber,
            int pageSize,
            string? search = null,
            string? action = null,
            string? entityType = null
        );
        Task<List<AuditLogDto>> GetAuditLogsByUserIdAsync(int userId);
    }
}
