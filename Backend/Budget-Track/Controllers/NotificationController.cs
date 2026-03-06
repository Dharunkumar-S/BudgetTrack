using Budget_Track.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Budget_Track.Controllers
{
    [Route("api/notifications")]
    [Authorize(Roles = "Admin,Manager,Employee")]
    public class NotificationController : BaseApiController
    {
        private readonly INotificationService _service;

        public NotificationController(INotificationService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications(
            [FromQuery] string? message = null,
            [FromQuery] string? status = null,
            [FromQuery] string sortOrder = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10
        )
        {
            try
            {
                var result = await _service.GetNotificationsByReceiverUserIdAsync(
                    UserId,
                    message,
                    status,
                    sortOrder,
                    pageNumber,
                    pageSize
                );
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { success = false, message = "Failed to retrieve notifications" }
                );
            }
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var count = await _service.GetUnreadCountAsync(UserId);
                return Ok(new { count });
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { success = false, message = "Failed to retrieve unread count" }
                );
            }
        }

        [HttpPut("read/{notificationID}")]
        public async Task<IActionResult> MarkAsRead(int notificationID)
        {
            try
            {
                var result = await _service.MarkNotificationAsReadAsync(notificationID, UserId);
                return Ok(new { success = result, message = "Notification is read" });
            }
            catch (Exception ex)
                when (ex.Message.Contains("Notification not found")
                    || ex.Message.Contains("does not exist")
                )
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
                when (ex.Message.Contains("Unauthorized") || ex.Message.Contains("not authorized"))
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
        }

        [HttpPut("readAll")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var count = await _service.MarkAllNotificationsAsReadAsync(UserId);
                return Ok(new { count, message = $"{count} notifications are read" });
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { success = false, message = "Failed to mark notifications as read" }
                );
            }
        }

        [HttpDelete("deleteAll")]
        public async Task<IActionResult> DeleteAllNotifications()
        {
            try
            {
                var count = await _service.DeleteAllNotificationsAsync(UserId);
                return Ok(new { count, message = $"{count} notifications deleted" });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        success = false,
                        message = "Failed to delete all notifications: " + ex.Message,
                    }
                );
            }
        }

        [HttpDelete("{notificationID:int}")]
        public async Task<IActionResult> DeleteNotification(int notificationID)
        {
            try
            {
                var result = await _service.DeleteNotificationAsync(notificationID, UserId);
                return Ok(new { success = result, message = "Notification deleted" });
            }
            catch (Exception ex)
                when (ex.Message.Contains("not found")
                    || ex.Message.Contains("already been deleted")
                )
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex) when (ex.Message.Contains("does not belong to user"))
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { success = false, message = "Failed to delete notification" }
                );
            }
        }
    }
}
