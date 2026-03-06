#nullable enable
namespace Budget_Track.Models.DTOs.Audit
{
    public class AuditLogDto
    {
        public int AuditLogID { get; set; }
        public int? UserID { get; set; }
        public string? UserName { get; set; }
        public string? EmployeeId { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public int EntityID { get; set; }
        public string Action { get; set; } = string.Empty;
        public object? OldValue { get; set; }
        public object? NewValue { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Notes { get; set; }
    }
}
