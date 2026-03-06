#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Budget_Track.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Budget_Track.Models.Entities
{
    [Table("tExpense")]
    [Index(nameof(BudgetID))]
    [Index(nameof(Status))]
    [Index(nameof(Title))]
    public class Expense
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ExpenseID { get; set; }

        [Required]
        public required int BudgetID { get; set; }

        [Required]
        public required int CategoryID { get; set; }

        [Required]
        [MaxLength(500)]
        public required string Title { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public required decimal Amount { get; set; }

        [MaxLength(200)]
        public string? MerchantName { get; set; }

        [Required]
        public required int SubmittedByUserID { get; set; }

        [Required]
        public required DateTime SubmittedDate { get; set; } = DateTime.UtcNow;

        [Required]
        public required ExpenseStatus Status { get; set; } = ExpenseStatus.Pending;

        public int? ManagerUserID { get; set; }
        public DateTime? StatusApprovedDate { get; set; }

        [MaxLength(500)]
        public string? RejectionReason { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        [MaxLength(1000)]
        public string? ApprovalComments { get; set; }

        [Required]
        public required DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; } = null!;

        public int? UpdatedByUserID { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedDate { get; set; }

        public int? DeletedByUserID { get; set; }

        public virtual Budget Budget { get; set; } = null!;
        public virtual Category Category { get; set; } = null!;
        public virtual User SubmittedByUser { get; set; } = null!;
        public virtual User? ApprovedByUser { get; set; }
        public virtual User? UpdatedByUser { get; set; }
        public virtual User? DeletedByUser { get; set; }
    }
}
