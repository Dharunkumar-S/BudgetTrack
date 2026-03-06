## Notification API

All routes are served from `http://localhost:5131/api/notifications` and require JWT Bearer authentication (Roles: Manager, Employee).

---

### Get Notifications

`GET http://localhost:5131/api/notifications`

#### Query Parameters

| Parameter  | Type   | Required | Description                                   |
| ---------- | ------ | -------- | --------------------------------------------- |
| message    | string | No       | Filter by notification message (partial)      |
| status     | string | No       | Filter by status (Read, Unread)               |
| sortOrder  | string | No       | Sort order: "asc" or "desc" (default: "desc") |
| pageNumber | int    | No       | Page number (default: 1)                      |
| pageSize   | int    | No       | Records per page (default: 10, max: 100)      |

#### Access: Manager, Employee (JWT Bearer)

**Description:** Retrieves notifications for the authenticated user. Returns paginated list of notifications with filtering and sorting options.

#### Status Codes

| Code | Status                | Message                          |
| ---- | --------------------- | -------------------------------- |
| 200  | OK                    | Notifications retrieved          |
| 401  | Unauthorized          | Unauthorized                     |
| 403  | Forbidden             | Forbidden                        |
| 500  | Internal Server Error | Failed to retrieve notifications |

#### Response Body

**200 OK**
```json
{
    "data": [
        {
            "notificationID": 1,
            "message": "Expense 'Monthly Cloud Hosting' has been approved",
            "type": "Expense",
            "status": "Unread",
            "receiverUserID": 7,
            "senderUserID": 2,
            "senderName": "Sanika Anil",
            "createdDate": "2026-02-26T14:30:00",
            "readDate": null
        },
        {
            "notificationID": 2,
            "message": "New budget 'Q1 2026 Operations' has been created",
            "type": "Budget",
            "status": "Read",
            "receiverUserID": 7,
            "senderUserID": 2,
            "senderName": "Sanika Anil",
            "createdDate": "2026-02-25T10:00:00",
            "readDate": "2026-02-25T11:30:00"
        }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalRecords": 25,
    "totalPages": 3,
    "hasNextPage": true,
    "hasPreviousPage": false
}
```

---

### Mark Notification as Read

`PUT http://localhost:5131/api/notifications/read/{notificationID}`

#### Route Parameters

| Parameter      | Type | Required | Description              |
| -------------- | ---- | -------- | ------------------------ |
| notificationID | int  | Yes      | Internal notification ID |

#### Access: Manager, Employee (JWT Bearer)

**Description:** Marks a single notification as read. Updates the read status and read timestamp.

#### Status Codes

| Code | Status                | Message                       |
| ---- | --------------------- | ----------------------------- |
| 200  | OK                    | Notification is read          |
| 401  | Unauthorized          | Unauthorized                  |
| 403  | Forbidden             | Not authorized to mark        |
| 404  | Not Found             | Notification not found        |
| 500  | Internal Server Error | Failed to update notification |

#### Response Body

**200 OK**
```json
{
    "success": true,
    "message": "Notification is read"
}
```

**403 Forbidden**
```json
{
    "success": false,
    "message": "Unauthorized"
}
```

**404 Not Found**
```json
{
    "success": false,
    "message": "Notification not found"
}
```

---

### Mark All Notifications as Read

`PUT http://localhost:5131/api/notifications/readAll`

#### Access: Manager, Employee (JWT Bearer)

**Description:** Marks all notifications for the authenticated user as read in a single operation.

#### Status Codes

| Code | Status                | Message                      |
| ---- | --------------------- | ---------------------------- |
| 200  | OK                    | Notifications marked as read |
| 401  | Unauthorized          | Unauthorized                 |
| 500  | Internal Server Error | Failed to mark notifications |

#### Response Body

**200 OK**
```json
{
    "count": 5,
    "message": "5 notifications are read"
}
```

---

### Delete Notification

`DELETE http://localhost:5131/api/notifications/{notificationID}`

#### Route Parameters

| Parameter      | Type | Required | Description              |
| -------------- | ---- | -------- | ------------------------ |
| notificationID | int  | Yes      | Internal notification ID |

#### Access: Manager, Employee (JWT Bearer)

**Description:** Permanently deletes a single notification for the authenticated user.

#### Status Codes

| Code | Status                | Message                       |
| ---- | --------------------- | ----------------------------- |
| 200  | OK                    | Notification deleted          |
| 401  | Unauthorized          | Unauthorized                  |
| 403  | Forbidden             | Not authorized to delete      |
| 404  | Not Found             | Notification not found        |
| 500  | Internal Server Error | Failed to delete notification |

#### Response Body

**200 OK**
```json
{
    "success": true,
    "message": "Notification deleted"
}
```

**403 Forbidden**
```json
{
    "success": false,
    "message": "Notification does not belong to user"
}
```

**404 Not Found**
```json
{
    "success": false,
    "message": "Notification not found or already been deleted"
}
```

---

### Delete All Notifications

`DELETE http://localhost:5131/api/notifications/deleteAll`

#### Access: Manager, Employee (JWT Bearer)

**Description:** Permanently deletes all notifications for the authenticated user in a single operation.

#### Status Codes

| Code | Status                | Message                        |
| ---- | --------------------- | ------------------------------ |
| 200  | OK                    | All notifications deleted      |
| 401  | Unauthorized          | Unauthorized                   |
| 500  | Internal Server Error | Failed to delete notifications |

#### Response Body

**200 OK**
```json
{
    "count": 15,
    "message": "15 notifications deleted"
}
```

`GET http://localhost:5131/api/notifications`

`http://localhost:5131/api/notifications?message=expense&sortOrder=desc&pageNumber=1&pageSize=10`

### Query Parameters

| Parameter  | Type   | Required | Message                                                   |
| ---------- | ------ | -------- | --------------------------------------------------------- |
| message    | string | No       | Filter by message content (uses `LIKE '%value%'`).        |
| sortOrder  | string | No       | Sort by `CreatedDate`: `asc` or `desc` (default: `desc`). |
| pageNumber | int    | No       | Page number for pagination (default: 1).                  |
| pageSize   | int    | No       | Number of records per page (default: 10).                 |

### Access Manager, Employee with Jwt bearer

**Note:** Retrieves all unread notifications for the authenticated user via `uspGetNotificationsByReceiverUserId`. The stored procedure filters by `Status = 1` (Unread) and `IsDeleted = 0`, joins with `tUser` to provide sender details (`SenderName`, `SenderEmployeeID`), and applies optional message filtering with `LIKE '%value%'`. The repository executes the procedure, retrieves all matching records, then applies in-memory pagination to return a `PagedResult<GetNotificationDto>`. This endpoint is designed to keep the user's inbox focused on pending actions such as expense approvals, rejections, or budget updates.

### Status Codes

| Code | Status                | Message                               |
| ---- | --------------------- | ------------------------------------- |
| 200  | OK                    | Notifications retrieved successfully  |
| 401  | Unauthorized          | Missing or invalid JWT                |
| 403  | Forbidden             | Caller lacks Manager or Employee role |
| 500  | Internal Server Error | Failed to retrieve notifications      |

### Response Body

```json
{
    "data": [
        {
            "notificationID": 1024,
            "type": 1,
            "message": "New expense pending approval: Engineering Operations - Travel - Rs.1500",
            "createdDate": "2026-02-25T14:30:00Z",
            "senderEmployeeID": "EMP2605",
            "senderName": "John Doe"
        }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalRecords": 1,
    "totalPages": 1,
    "hasNextPage": false,
    "hasPreviousPage": false,
    "firstPage": 1,
    "lastPage": 1,
    "nextPage": null,
    "previousPage": null,
    "firstItemIndex": 1,
    "lastItemIndex": 1,
    "currentPageItemCount": 1,
    "isFirstPage": true,
    "isLastPage": true
}
```
*Note: The `type` field maps to `NotificationType` enum: 1=ExpenseApprovalReminder, 2=ExpenseApproved, 3=ExpenseRejected, 4=BudgetCreated, 5=BudgetUpdated, 6=BudgetDeleted. Pagination is applied in-memory after retrieving all unread notifications from the stored procedure.*

`PUT http://localhost:5131/api/notifications/read/{notificationID}`

`http://localhost:5131/api/notifications/read/1024`

### Access Manager, Employee with Jwt bearer

**Note:** Marks a single notification as read using `uspMarkNotificationAsRead`. The stored procedure validates that the notification exists, belongs to the authenticated user (`ReceiverUserID`), is currently unread (`Status = 1`), and is not soft-deleted. Upon validation, it updates the status to `2` (Read), sets `ReadDate` to current UTC time, and wraps the operation in a transaction for atomicity. If the notification belongs to another user or doesn't exist, the procedure raises an error that the controller surfaces as a 401 or 404 response.

### Status Codes

| Code | Status                | Message                                           |
| ---- | --------------------- | ------------------------------------------------- |
| 200  | OK                    | Notification is read                              |
| 401  | Unauthorized          | Notification not found or does not belong to user |
| 403  | Forbidden             | Caller lacks Manager or Employee role             |
| 404  | Not Found             | Notification not found                            |
| 500  | Internal Server Error | Failed to mark notification as read               |

### Response Body

**200 OK**

```json
{
    "success": true,
    "message": "Notification is read"
}
```
*Note: Returned when `uspMarkNotificationAsRead` successfully updates the notification status from Unread to Read.*

**401 Unauthorized**

```json
{
    "success": false,
    "message": "Notification not found or does not belong to user"
}
```
*Note: Triggered when the stored procedure validates that the notification exists but the `ReceiverUserID` does not match the authenticated user's ID.*

**404 Not Found**

```json
{
    "success": false,
    "message": "Notification not found"
}
```
*Note: Returned when the notification does not exist or has been soft-deleted.*

`PUT http://localhost:5131/api/notifications/readAll`

### Access Manager, Employee with Jwt bearer

**Note:** Bulk-marks all unread notifications for the authenticated user using `uspMarkAllNotificationsAsRead`. The stored procedure executes a transactional `UPDATE` statement that targets all notifications where `ReceiverUserID` matches the authenticated user, `Status = 1` (Unread), and `IsDeleted = 0`. It sets `Status = 2` (Read) and captures `ReadDate` with `GETUTCDATE()`. The procedure returns the count of modified rows via an `OUTPUT` parameter (`@@ROWCOUNT`), which the controller includes in the response message. This endpoint is useful for batch operations, allowing users to clear their entire notification inbox in a single atomic operation.

### Status Codes

| Code | Status                | Message                               |
| ---- | --------------------- | ------------------------------------- |
| 200  | OK                    | [count] notifications are read        |
| 401  | Unauthorized          | Missing or invalid JWT                |
| 403  | Forbidden             | Caller lacks Manager or Employee role |
| 500  | Internal Server Error | Failed to mark notifications as read  |

### Response Body

**200 OK**

```json
{
    "count": 5,
    "message": "5 notifications are read"
}
```
*Note: The `count` represents the number of notifications successfully transitioned from `Unread (1)` to `Read (2)`. If the user has no unread notifications, the count will be `0`.*
