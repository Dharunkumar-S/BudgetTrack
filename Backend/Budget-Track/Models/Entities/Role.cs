#nullable enable
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Budget_Track.Models.Entities
{
    [Table("tRole")]
    [Index(nameof(RoleName), IsUnique = true)]
    public class Role
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RoleID { get; set; }

        [Required]
        [MaxLength(100)]
        public required string RoleName { get; set; }

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
        public virtual ICollection<User> Users { get; set; } = new List<User>();
    }
}
