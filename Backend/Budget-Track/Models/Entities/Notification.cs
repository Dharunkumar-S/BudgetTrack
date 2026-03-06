#nullable enable
using Budget_Track.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Budget_Track.Models.Entities
{
    [Table("tNotification")]
    [Index(nameof(ReceiverUserID))]
    public class Notification
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int NotificationID { get; set; }

        [Required]
        public required int SenderUserID { get; set; }

        [Required]
        public required int ReceiverUserID { get; set; }

        [Required]
        public required NotificationType Type { get; set; }

        [Required]
        [MaxLength(500)]
        public required string Message { get; set; }

        [Required]
        public required NotificationStatus Status { get; set; } = NotificationStatus.Unread;

        [Required]
        public required DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ReadDate { get; set; }

        [MaxLength(50)]
        public string? RelatedEntityType { get; set; }
        public int? RelatedEntityID { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedDate { get; set; }

        public virtual User? Sender { get; set; }
        public virtual User? Receiver { get; set; }
    }
}
