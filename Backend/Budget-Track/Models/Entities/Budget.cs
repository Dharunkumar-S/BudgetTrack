#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Budget_Track.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Budget_Track.Models.Entities
{
    [Table("tBudget")]
    [Index(nameof(DepartmentID))]
    [Index(nameof(Title), IsUnique = true)]
    [Index(nameof(Code), IsUnique = true)]
    public class Budget
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BudgetID { get; set; }

        [Required]
        [MaxLength(200)]
        public required string Title { get; set; }

        [MaxLength(50)]
        public string? Code { get; set; }

        [Required]
        public required int DepartmentID { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public required decimal AmountAllocated { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountSpent { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountRemaining { get; set; } = 0;

        [Required]
        public required DateTime StartDate { get; set; }

        [Required]
        public required DateTime EndDate { get; set; }

        [Required]
        public required BudgetStatus Status { get; set; } = BudgetStatus.Active;

        [Required]
        public required int CreatedByUserID { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [Required]
        public required DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; } = null!;

        public int? UpdatedByUserID { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedDate { get; set; }

        public int? DeletedByUserID { get; set; }

        // Navigation properties
        public virtual Department Department { get; set; } = null!;
        public virtual User? CreatedByUser { get; set; }
        public virtual User? UpdatedByUser { get; set; }
        public virtual User? DeletedByUser { get; set; }
        public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    }
}