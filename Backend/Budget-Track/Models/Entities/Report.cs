#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Budget_Track.Models.Enums;

namespace Budget_Track.Models.Entities
{
    [Table("tReport")]
    public class Report
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReportID { get; set; }

        [MaxLength(200)]
        [Required]
        public required string Title { get; set; } = string.Empty;

        [Required]
        public ReportScopeType? Scope { get; set; }=ReportScopeType.Period;

        [Column(TypeName = "nvarchar(max)")]
        public string? Metrics { get; set; }

        [Required]
        public required DateTime GeneratedDate { get; set; } = DateTime.UtcNow;

        [Required]
        public required int GeneratedByUserID { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedDate { get; set; }
        public int? DeletedByUserID { get; set; }

        public virtual User? GeneratedByUser { get; set; }
        public virtual User? DeletedByUser { get; set; }
    }
}