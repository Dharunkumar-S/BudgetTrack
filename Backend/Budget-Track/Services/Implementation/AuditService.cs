using Budget_Track.Models.DTOs.Audit;
using Budget_Track.Models.DTOs.Pagination;
using Budget_Track.Repositories.Interfaces;
using Budget_Track.Services.Interfaces;

namespace Budget_Track.Services.Implementation
{
    public class AuditService : IAuditService
    {
        private readonly IAuditRepository _auditRepository;

        public AuditService(IAuditRepository auditRepository)
        {
            _auditRepository = auditRepository;
        }

        public async Task<List<AuditLogDto>> GetAllAuditLogsAsync()
        {
            return await _auditRepository.GetAllAuditLogsAsync();
        }

        public async Task<PagedResult<AuditLogDto>> GetAllAuditLogsPaginatedAsync(
            int pageNumber,
            int pageSize,
            string? search = null,
            string? action = null,
            string? entityType = null
        )
        {
            if (pageNumber <= 0)
                throw new ArgumentException(
                    "Page number must be greater than 0",
                    nameof(pageNumber)
                );

            if (pageSize <= 0 || pageSize > 100)
                throw new ArgumentException(
                    "Page size must be between 1 and 100",
                    nameof(pageSize)
                );

            return await _auditRepository.GetAllAuditLogsPaginatedAsync(
                pageNumber,
                pageSize,
                search,
                action,
                entityType
            );
        }

        public async Task<List<AuditLogDto>> GetAuditLogsByUserIdAsync(int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("User ID must be greater than 0", nameof(userId));
            }

            return await _auditRepository.GetAuditLogsByUserIdAsync(userId);
        }
    }
}
