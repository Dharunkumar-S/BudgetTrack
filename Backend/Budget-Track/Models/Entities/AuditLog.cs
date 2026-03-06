#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Budget_Track.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Budget_Track.Models.Entities
{
    [Table("tAuditLog")]
    [Index(nameof(UserID))]
    [Index(nameof(EntityType))]
    public class AuditLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AuditLogID { get; set; }

        public int? UserID { get; set; }

        [Required]
        [MaxLength(50)]
        public required string EntityType { get; set; }

        [Required]
        public required int EntityID { get; set; }

        [Required]
        public required AuditAction Action { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? OldValue { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? NewValue { get; set; }


        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        public required DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public virtual User? User { get; set; }
    }
}
