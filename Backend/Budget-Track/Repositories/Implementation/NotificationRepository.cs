using Budget_Track.Data;
using Budget_Track.Models.DTOs.Notification;
using Budget_Track.Models.DTOs.Pagination;
using Budget_Track.Models.Enums;
using Budget_Track.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Budget_Track.Repositories.Implementation
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly BudgetTrackDbContext _context;

        public NotificationRepository(BudgetTrackDbContext context)
        {
            _context = context;
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
            var totalRecordsParam = new SqlParameter("@TotalRecords", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };

            var data = await _context.Database
                .SqlQueryRaw<GetNotificationDto>(
                    @"EXEC uspGetNotificationsByReceiverUserId
                        @ReceiverUserID, @Message, @Status,
                        @SortOrder, @PageNumber, @PageSize,
                        @TotalRecords OUTPUT",
                    new SqlParameter("@ReceiverUserID", receiverUserID),
                    new SqlParameter("@Message", (object?)message ?? DBNull.Value),
                    new SqlParameter("@Status", (object?)status ?? DBNull.Value),
                    new SqlParameter("@SortOrder", sortOrder),
                    new SqlParameter("@PageNumber", pageNumber),
                    new SqlParameter("@PageSize", pageSize),
                    totalRecordsParam
                )
                .ToListAsync();

            var totalRecords = totalRecordsParam.Value == DBNull.Value
                ? 0
                : (int)totalRecordsParam.Value;

            return PagedResult<GetNotificationDto>.Create(data, pageNumber, pageSize, totalRecords);
        }

        public async Task<int> GetUnreadCountAsync(int receiverUserID)
        {
            var unreadCountParam = new SqlParameter("@UnreadCount", SqlDbType.Int)
            {
                Direction = ParameterDirection.Output
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC uspGetUnreadNotificationCount @ReceiverUserID, @UnreadCount OUTPUT",
                new SqlParameter("@ReceiverUserID", receiverUserID),
                unreadCountParam
            );

            return unreadCountParam.Value == DBNull.Value ? 0 : (int)unreadCountParam.Value;
        }

        public async Task<bool> MarkNotificationAsReadAsync(int notificationID, int receiverUserID)
        {
            await _context.Database.ExecuteSqlRawAsync(
                @"EXEC uspMarkNotificationAsRead
                    @NotificationID, @ReceiverUserID",
                new SqlParameter("@NotificationID", notificationID),
                new SqlParameter("@ReceiverUserID", receiverUserID)
            );

            return true;
        }

        public async Task<int> MarkAllNotificationsAsReadAsync(int receiverUserID)
        {
            var updatedCountParam = new SqlParameter
            {
                ParameterName = "@UpdatedCount",
                SqlDbType = System.Data.SqlDbType.Int,
                Direction = System.Data.ParameterDirection.Output,
            };

            await _context.Database.ExecuteSqlRawAsync(
                @"EXEC uspMarkAllNotificationsAsRead
                    @ReceiverUserID, @UpdatedCount OUTPUT",
                new SqlParameter("@ReceiverUserID", receiverUserID),
                updatedCountParam
            );

            return (int)updatedCountParam.Value;
        }

        public async Task<bool> DeleteNotificationAsync(int notificationID, int receiverUserID)
        {
            await _context.Database.ExecuteSqlRawAsync(
                @"EXEC uspDeleteNotification @NotificationID, @ReceiverUserID",
                new SqlParameter("@NotificationID", notificationID),
                new SqlParameter("@ReceiverUserID", receiverUserID)
            );

            return true;
        }

        public async Task<int> DeleteAllNotificationsAsync(int receiverUserID)
        {
            var deletedCountParam = new SqlParameter
            {
                ParameterName = "@DeletedCount",
                SqlDbType = System.Data.SqlDbType.Int,
                Direction = System.Data.ParameterDirection.Output,
            };

            await _context.Database.ExecuteSqlRawAsync(
                @"EXEC uspDeleteAllNotifications @ReceiverUserID, @DeletedCount OUTPUT",
                new SqlParameter("@ReceiverUserID", receiverUserID),
                deletedCountParam
            );

            return deletedCountParam.Value == DBNull.Value ? 0 : (int)deletedCountParam.Value;
        }
    }
}
