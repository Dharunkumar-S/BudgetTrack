#nullable enable
using Budget_Track.Models.Enums;

namespace Budget_Track.Models.DTOs.Notification
{
    public class GetNotificationDto
    {
        public int NotificationID { get; set; }
        public NotificationType Type { get; set; }
        public required string Message { get; set; }
        public DateTime CreatedDate { get; set; }
        public required string SenderEmployeeID { get; set; }
        public required string SenderName { get; set; }

        // Status: 1=Unread, 2=Read — returned by the stored procedure
        public int Status { get; set; } = (int)NotificationStatus.Unread;

        // Computed helper for the frontend
        public bool IsRead => Status == (int)NotificationStatus.Read;
    }
}
