using Budget_Track.Models.DTOs.Notification;
using Budget_Track.Models.DTOs.Pagination;
using Budget_Track.Repositories.Interfaces;
using Budget_Track.Services.Interfaces;

namespace Budget_Track.Services.Implementation
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repository;

        public NotificationService(INotificationRepository repository)
        {
            _repository = repository;
        }

        public async Task<PagedResult<GetNotificationDto>> GetNotificationsByReceiverUserIdAsync(
            int receiverUserID,
            string? message = null,
            string? status = null,
            string sortOrder = "desc",
            int pageNumber = 1,
            int pageSize = 10
        )
        {
            return await _repository.GetNotificationsByReceiverUserIdAsync(
                receiverUserID,
                message,
                status,
                sortOrder,
                pageNumber,
                pageSize
            );
        }

        public async Task<int> GetUnreadCountAsync(int receiverUserID)
        {
            return await _repository.GetUnreadCountAsync(receiverUserID);
        }

        public async Task<bool> MarkNotificationAsReadAsync(int notificationID, int receiverUserID)
        {
            return await _repository.MarkNotificationAsReadAsync(notificationID, receiverUserID);
        }

        public async Task<int> MarkAllNotificationsAsReadAsync(int receiverUserID)
        {
            return await _repository.MarkAllNotificationsAsReadAsync(receiverUserID);
        }

        public async Task<bool> DeleteNotificationAsync(int notificationID, int receiverUserID)
        {
            return await _repository.DeleteNotificationAsync(notificationID, receiverUserID);
        }

        public async Task<int> DeleteAllNotificationsAsync(int receiverUserID)
        {
            return await _repository.DeleteAllNotificationsAsync(receiverUserID);
        }
    }
}
