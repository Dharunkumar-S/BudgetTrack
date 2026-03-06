#nullable enable
using Budget_Track.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Budget_Track.Models.Entities
{
    [Table("tUser")]
    [Index(nameof(Email), IsUnique = true)]
    [Index(nameof(EmployeeID), IsUnique = true)]
    [Index(nameof(DepartmentID))]
    [Index(nameof(RoleID))]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserID { get; set; }

        [Required]
        [MaxLength(50)]
        public required string FirstName { get; set; }

        [Required]
        [MaxLength(50)]
        public required string LastName { get; set; }

        [Required]
        [MaxLength(50)]
        public required string EmployeeID { get; set; }

        [Required]
        [MaxLength(100)]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [MaxLength(500)]
        public required string PasswordHash { get; set; }

        [Required]
        public required int DepartmentID { get; set; }

        [Required]
        public required int RoleID { get; set; }

        [Required]
        public required UserStatus Status { get; set; } = UserStatus.Active;

        public int? ManagerID { get; set; }

        [MaxLength(500)]
        public string? RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiryTime { get; set; }

        public DateTime? LastLoginDate { get; set; }

        [Required]
        public required DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public int? CreatedByUserID { get; set; }

        public DateTime? UpdatedDate { get; set; } = null!;

        public int? UpdatedByUserID { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedDate { get; set; }

        public int? DeletedByUserID { get; set; }

        public virtual Department Department { get; set; } = null!;
        public virtual Role Role { get; set; } = null!;
        public virtual User? Manager { get; set; }
        public virtual ICollection<User> Subordinates { get; set; } = new List<User>();
        public virtual User? CreatedByUser { get; set; }
        public virtual User? UpdatedByUser { get; set; }
        public virtual User? DeletedByUser { get; set; }
        public virtual ICollection<Budget> BudgetsCreated { get; set; } = new List<Budget>();
        public virtual ICollection<Expense> ExpensesSubmitted { get; set; } = new List<Expense>();
        public virtual ICollection<Expense> ExpensesApproved { get; set; } = new List<Expense>();
        public virtual ICollection<Report> ReportsGenerated { get; set; } = new List<Report>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}
