#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Budget_Track.Models.Entities
{
    [Table("tCategory")]
    [Index(nameof(CategoryName), IsUnique = true)]
    [Index(nameof(CategoryCode), IsUnique = true)]
    public class Category
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CategoryID { get; set; }

        [Required]
        [MaxLength(100)]
        public required string CategoryName { get; set; }

        [Required]
        [MaxLength(50)]
        public required string CategoryCode { get; set; }

        [Required]
        public required bool IsActive { get; set; } = true;

        [Required]
        public required DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public int? CreatedByUserID { get; set; }

        public DateTime? UpdatedDate { get; set; } = null!;

        public int? UpdatedByUserID { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedDate { get; set; }

        public int? DeletedByUserID { get; set; }

        public virtual User? CreatedByUser { get; set; }
        public virtual User? UpdatedByUser { get; set; }
        public virtual User? DeletedByUser { get; set; }
        public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    }
}