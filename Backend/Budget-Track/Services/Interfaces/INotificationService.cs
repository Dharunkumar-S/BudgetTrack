using Budget_Track.Models.DTOs.Notification;
using Budget_Track.Models.DTOs.Pagination;

namespace Budget_Track.Services.Interfaces
{
    public interface INotificationService
    {
        Task<PagedResult<GetNotificationDto>> GetNotificationsByReceiverUserIdAsync(
            int receiverUserID,
            string? message = null,
            string? status = null,
            string sortOrder = "desc",
            int pageNumber = 1,
            int pageSize = 10
        );

        Task<int> GetUnreadCountAsync(int receiverUserID);
        Task<bool> MarkNotificationAsReadAsync(int notificationID, int receiverUserID);
        Task<int> MarkAllNotificationsAsReadAsync(int receiverUserID);
        Task<bool> DeleteNotificationAsync(int notificationID, int receiverUserID);
        Task<int> DeleteAllNotificationsAsync(int receiverUserID);
    }
}
