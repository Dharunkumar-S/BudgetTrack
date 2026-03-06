## Audit API

All routes are served from `http://localhost:5131/api/audits` and require Admin role with JWT Bearer authentication.

---

### Get All Audit Logs (Paginated)

`GET http://localhost:5131/api/audits`

#### Query Parameters

| Parameter  | Type   | Required | Description                                                          |
| ---------- | ------ | -------- | -------------------------------------------------------------------- |
| pageNumber | int    | No       | Page number (default: 1)                                             |
| pageSize   | int    | No       | Records per page (default: 10, max: 100)                             |
| search     | string | No       | Search in audit log details (partial match)                          |
| action     | string | No       | Filter by action type: `Create`, `Update`, `Delete`                  |
| entityType | string | No       | Filter by entity type: `Budget`, `Expense`, `Category`, `Department` |

#### Access: Admin only (JWT Bearer)

**Description:** Retrieves a paginated list of all audit logs in the system. Audit logs track all system actions including user activities, data changes, and administrative operations. Essential for compliance, security monitoring, and troubleshooting.

#### Status Codes

| Code | Status                | Message                       |
| ---- | --------------------- | ----------------------------- |
| 200  | OK                    | Audit logs retrieved          |
| 400  | Bad Request           | Invalid pagination parameters |
| 401  | Unauthorized          | Unauthorized                  |
| 403  | Forbidden             | Admin role required           |
| 500  | Internal Server Error | Failed to retrieve audit logs |

#### Response Body

**200 OK**
```json
{
    "data": [
        {
            "auditID": 1,
            "userID": 2,
            "userName": "Sanika Anil",
            "action": "Create",
            "entityType": "Budget",
            "entityID": 1,
            "oldValue": null,
            "newValue": "{\"budgetID\": 1, \"title\": \"Engineering Operations\", \"code\": \"BT2601\", \"amountAllocated\": 5000000.00}",
            "timestamp": "2026-02-25T01:24:20.3843916",
            "ipAddress": "192.168.1.100",
            "details": "Created new budget"
        },
        {
            "auditID": 2,
            "userID": 2,
            "userName": "Sanika Anil",
            "action": "Update",
            "entityType": "Budget",
            "entityID": 1,
            "oldValue": "{\"amountAllocated\": 5000000.00}",
            "newValue": "{\"amountAllocated\": 5500000.00}",
            "timestamp": "2026-02-26T10:30:45.1234567",
            "ipAddress": "192.168.1.100",
            "details": "Updated budget allocation"
        }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalRecords": 245,
    "totalPages": 25,
    "hasNextPage": true,
    "hasPreviousPage": false
}
```

**400 Bad Request**
```json
{
    "message": "Invalid page number or page size"
}
```

---

### Get Audit Logs by User

`GET http://localhost:5131/api/audits/{userId}`

#### Route Parameters

| Parameter | Type | Required | Description      |
| --------- | ---- | -------- | ---------------- |
| userId    | int  | Yes      | Internal user ID |

#### Access: Admin only (JWT Bearer)

**Description:** Retrieves all audit logs for a specific user. Useful for tracking user activities and identifying who performed what actions.

#### Status Codes

| Code | Status                | Message                       |
| ---- | --------------------- | ----------------------------- |
| 200  | OK                    | Audit logs retrieved          |
| 400  | Bad Request           | Invalid user ID               |
| 401  | Unauthorized          | Unauthorized                  |
| 403  | Forbidden             | Admin role required           |
| 500  | Internal Server Error | Failed to retrieve audit logs |

#### Response Body

**200 OK**
```json
[
    {
        "auditID": 1,
        "userID": 2,
        "userName": "Sanika Anil",
        "action": "Create",
        "entityType": "Budget",
        "entityID": 1,
        "oldValue": null,
        "newValue": "{\"budgetID\": 1, \"title\": \"Engineering Operations\", \"code\": \"BT2601\"}",
        "timestamp": "2026-02-25T01:24:20.3843916",
        "ipAddress": "192.168.1.100",
        "details": "Created new budget"
    },
    {
        "auditID": 5,
        "userID": 2,
        "userName": "Sanika Anil",
        "action": "Update",
        "entityType": "Expense",
        "entityID": 3,
        "oldValue": "{\"status\": \"Pending\"}",
        "newValue": "{\"status\": \"Approved\"}",
        "timestamp": "2026-02-26T14:15:30.9876543",
        "ipAddress": "192.168.1.100",
        "details": "Expense status updated to Approved"
    }
]
```

**200 OK (No Records)**
```json
[]
```

`GET http://localhost:5131/api/audits`

### Access Admin,Manager with Jwt bearer

**Note:** Retrieves all audit log entries from the system using the `GetAllAuditLogsAsync` repository method. This comprehensive auditing endpoint queries the tAuditLog table with eager loading of User entities to provide complete audit trail information for compliance and security monitoring. The method returns audit logs ordered by creation date in descending order (most recent first), including user actions, entity changes, before/after states, and timestamps. Automatically parses JSON-formatted OldValue and NewValue fields stored during create, update, and delete operations across all entities (budgets, expenses, users, etc.). Essential for compliance reporting, security audits, change tracking, and forensic analysis. Restricted to administrators and managers for sensitive audit data protection.

### Status Codes

| Code | Status                | Message                       |
| ---- | --------------------- | ----------------------------- |
| 200  | OK                    | Audit logs retrieved          |
| 401  | Unauthorized          | Unauthorized                  |
| 403  | Forbidden             | Forbidden                     |
| 500  | Internal Server Error | Failed to retrieve audit logs |

### Response Body

**200 OK**
```json
{
    "data": [
        {
            "auditLogID": 27,
            "userID": 3,
            "userName": "Rahul K",
            "entityType": "Expense",
            "entityID": 76,
            "action": "Create",
            "oldValue": null,
            "newValue": {
                "ExpenseID": 76,
                "BudgetID": 1,
                "CategoryID": 2,
                "Title": "Category",
                "Amount": 1000.00,
                "MerchantName": "",
                "SubmittedByUserID": 3,
                "SubmittedDate": "2026-02-25T06:35:23.1700000",
                "Status": 1,
                "CreatedDate": "2026-02-25T06:35:23.1700000",
                "UpdatedDate": "2026-02-25T06:35:23.1700000"
            },
            "timestamp": "2026-02-25T06:35:23.1733333",
            "notes": "Expense created: Engineering Operations - Cloud Infrastructure - Rs.1000.00"
        },
        {
            "auditLogID": 13,
            "userID": 2,
            "userName": "Sanika Anil",
            "entityType": "Expense",
            "entityID": 1,
            "action": "Update",
            "oldValue": {
                "ExpenseID": 1,
                "BudgetID": 1,
                "CategoryID": 8,
                "Title": "Database Hosting Service",
                "Amount": 122029.00,
                "MerchantName": "Salesforce Inc",
                "SubmittedByUserID": 7,
                "SubmittedDate": "2026-02-02T01:24:20.3843916",
                "Status": 1,
                "CreatedDate": "2026-02-25T01:24:20.3843916",
                "UpdatedDate": "2026-02-25T01:24:20.3843916"
            },
            "newValue": {
                "ExpenseID": 1,
                "BudgetID": 1,
                "CategoryID": 8,
                "Title": "Database Hosting Service",
                "Amount": 122029.00,
                "MerchantName": "Salesforce Inc",
                "SubmittedByUserID": 7,
                "SubmittedDate": "2026-02-02T01:24:20.3843916",
                "Status": 2,
                "ManagerUserID": 2,
                "StatusApprovedDate": "2026-02-25T06:06:35.0766667",
                "ApprovalComments": "",
                "CreatedDate": "2026-02-25T01:24:20.3843916",
                "UpdatedDate": "2026-02-25T06:06:35.0766667"
            },
            "timestamp": "2026-02-25T06:06:35.08",
            "notes": "Expense updated: Engineering Operations - Open Source Support - Approved"
        },
        {
            "auditLogID": 11,
            "userID": 2,
            "userName": "Sanika Anil",
            "entityType": "Budget",
            "entityID": 8,
            "action": "Delete",
            "oldValue": {
                "BudgetID": 8,
                "Title": "Marketing Q2",
                "Code": "MKT002",
                "DepartmentID": 2,
                "AmountAllocated": 10000.00,
                "AmountSpent": 0.00,
                "AmountRemaining": 10000.00,
                "StartDate": "2026-01-01T09:00:00",
                "EndDate": "2026-04-30T18:00:00",
                "Status": 1,
                "IsDeleted": false
            },
            "newValue": null,
            "timestamp": "2026-02-25T06:06:18.29",
            "notes": "Budget Deleted: MKT002 Marketing Q2"
        },
        {
            "auditLogID": 3,
            "userID": 2,
            "userName": "Sanika Anil",
            "entityType": "Budget",
            "entityID": 6,
            "action": "Update",
            "oldValue": {
                "BudgetID": 6,
                "Title": "Marketing Q1",
                "Code": "MKT001",
                "DepartmentID": 3,
                "AmountAllocated": 10000.00,
                "AmountSpent": 0.00,
                "AmountRemaining": 10000.00,
                "StartDate": "2026-01-01T09:00:00",
                "EndDate": "2026-04-30T18:00:00",
                "Status": 1,
                "UpdatedDate": "2026-02-25T05:18:32.1166667"
            },
            "newValue": {
                "BudgetID": 6,
                "Title": "Marketing Q1",
                "Code": "MKT001",
                "DepartmentID": 2,
                "AmountAllocated": 10000.00,
                "AmountSpent": 0.00,
                "AmountRemaining": 10000.00,
                "StartDate": "2026-01-01T09:00:00",
                "EndDate": "2026-11-30T18:00:00",
                "Status": 2,
                "UpdatedDate": "2026-02-25T05:58:12.2100000"
            },
            "timestamp": "2026-02-25T05:58:12.2266667",
            "notes": "Budget Updated: MKT001 Marketing Q1"
        },
        {
            "auditLogID": 1,
            "userID": 2,
            "userName": "Sanika Anil",
            "entityType": "Budget",
            "entityID": 6,
            "action": "Create",
            "oldValue": null,
            "newValue": {
                "BudgetID": 6,
                "Title": "Marketing Q1",
                "Code": "MKT001",
                "DepartmentID": 3,
                "AmountAllocated": 10000.00,
                "AmountSpent": 0.00,
                "AmountRemaining": 10000.00,
                "StartDate": "2026-01-01T09:00:00",
                "EndDate": "2026-04-30T18:00:00",
                "Status": 1,
                "CreatedDate": "2026-02-25T05:18:32.127"
            },
            "timestamp": "2026-02-25T05:18:32.1333333",
            "notes": "Budget Created: MKT001 Marketing Q1"
        }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalRecords": 5,
    "totalPages": 1,
    "hasNextPage": false,
    "hasPreviousPage": false,
    "firstPage": 1,
    "lastPage": 1,
    "nextPage": null,
    "previousPage": null,
    "firstItemIndex": 1,
    "lastItemIndex": 5,
    "currentPageItemCount": 5,
    "isFirstPage": true,
    "isLastPage": true
}
```
*Note: Returned when GetAllAuditLogsAsync successfully retrieves all audit logs with JSON-parsed OldValue/NewValue and user information*

**500 Internal Server Error**
```json
{
    "message": "Failed to retrieve audit logs"
}
```
*Note: Triggered when database query fails or JSON parsing errors occur during audit log retrieval*

---

`GET http://localhost:5131/api/audits/{userId}`

### Access Admin,Manager with Jwt bearer

**Note:** Retrieves audit log entries for a specific user using the `GetAuditLogsByUserIdAsync` repository method. This filtered auditing endpoint queries the tAuditLog table filtering by UserID to track all actions performed by a particular user across the system. Essential for individual user activity monitoring, accountability tracking, and investigating specific user behaviors. Returns logs ordered by creation date in descending order with parsed JSON values for OldValue and NewValue fields. If no audit logs exist for the user, returns an empty array rather than an error, allowing graceful handling of new users without activity history. Restricted to administrators and managers for user activity oversight and compliance.

### Status Codes

| Code | Status                | Message                       |
| ---- | --------------------- | ----------------------------- |
| 200  | OK                    | Audit logs retrieved          |
| 400  | Bad Request           | Invalid user ID               |
| 401  | Unauthorized          | Unauthorized                  |
| 403  | Forbidden             | Forbidden                     |
| 500  | Internal Server Error | Failed to retrieve audit logs |

### Response Body

**200 OK**
```json
[
    {
        "auditLogID": 10,
        "userID": 3,
        "userName": "John Doe",
        "entityType": "Expense",
        "entityID": 12,
        "action": "Create",
        "oldValue": null,
        "newValue": {
            "title": "Team Lunch",
            "amount": 850.00,
            "categoryID": 5,
            "budgetID": 1,
            "status": 1
        },
        "timestamp": "2026-02-25T12:45:30.5678901",
        "notes": "New expense submitted"
    },
    {
        "auditLogID": 8,
        "userID": 3,
        "userName": "John Doe",
        "entityType": "Expense",
        "entityID": 8,
        "action": "Update",
        "oldValue": {
            "title": "Software License",
            "amount": 1200.00,
            "status": 1
        },
        "newValue": {
            "title": "Software License - Annual",
            "amount": 1200.00,
            "status": 2
        },
        "timestamp": "2026-02-24T09:15:45.1234567",
        "notes": "Expense approved by manager"
    }
]
```
*Note: Returned when GetAuditLogsByUserIdAsync successfully retrieves user-specific audit logs with parsed JSON values*

**200 OK (Empty results)**
```json
[]
```
*Note: Returned when user exists but has no audit log entries, handled gracefully by the controller*

**400 Bad Request**
```json
{
    "message": "User ID must be greater than 0 (Parameter 'userId')"
}
```
*Note: Triggered by AuditService validation when userId parameter is less than or equal to 0. Controller catches ArgumentException and returns BadRequest*

**500 Internal Server Error**
```json
{
    "message": "Failed to retrieve audit logs"
}
```
*Note: Triggered when database query fails or JSON parsing errors occur during user-specific audit log retrieval*
