## Budget API

All routes are served from `http://localhost:5131/api/budgets` and require JWT Bearer authentication unless otherwise specified.

---

### Get All Budgets (Admin)

`GET http://localhost:5131/api/budgets/admin`

#### Query Parameters

| Parameter  | Type      | Required | Description                                          |
| ---------- | --------- | -------- | ---------------------------------------------------- |
| title      | string    | No       | Filter budgets by title (partial match)              |
| code       | string    | No       | Filter budgets by code (partial match)               |
| status     | List<int> | No       | Filter by status (1=Active, 2=Closed)                |
| isDeleted  | bool      | No       | Filter by deletion status                            |
| sortBy     | string    | No       | Sort field: "CreatedDate" (default), "Title", "Code" |
| sortOrder  | string    | No       | Sort order: "asc" or "desc" (default: "desc")        |
| pageNumber | int       | No       | Page number (default: 1)                             |
| pageSize   | int       | No       | Records per page (default: 10, max: 100)             |

#### Access: Admin only (JWT Bearer)

**Description:** Admin-only endpoint providing comprehensive budget oversight. Retrieves all budgets from the system, including both active and soft-deleted budgets. Includes real-time calculated metrics like utilization percentage, days remaining, expiration status, and over-budget indicators. Essential for system-wide budget management and audit purposes.

#### Status Codes

| Code | Status                | Message                        |
| ---- | --------------------- | ------------------------------ |
| 200  | OK                    | Budgets retrieved successfully |
| 401  | Unauthorized          | Unauthorized                   |
| 403  | Forbidden             | Forbidden                      |
| 500  | Internal Server Error | Failed to retrieve budgets     |

#### Response Body

**200 OK**
```json
{
    "data": [
        {
            "budgetID": 1,
            "title": "Engineering Operations",
            "code": "BT2601",
            "departmentID": 1,
            "departmentName": "Engineering Operations",
            "amountAllocated": 5000000.00,
            "amountSpent": 1068348.00,
            "amountRemaining": 3931652.00,
            "utilizationPercentage": 21.37,
            "startDate": "2026-02-25T01:24:20.3843916",
            "endDate": "2027-02-25T01:24:20.3843916",
            "status": 1,
            "statusName": "Active",
            "notes": null,
            "createdByUserID": 2,
            "createdByName": "Sanika Anil",
            "createdByEmployeeID": "MGR2601",
            "createdDate": "2026-02-25T01:24:20.3843916",
            "updatedDate": "2026-02-25T06:26:11.2566667",
            "daysRemaining": 365,
            "isExpired": false,
            "isOverBudget": false
        }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalRecords": 1,
    "totalPages": 1,
    "hasNextPage": false,
    "hasPreviousPage": false
}
```

---

### Get Budgets (Role-Based)

`GET http://localhost:5131/api/budgets`

#### Query Parameters

| Parameter  | Type      | Required | Description                                          |
| ---------- | --------- | -------- | ---------------------------------------------------- |
| title      | string    | No       | Filter budgets by title (partial match)              |
| code       | string    | No       | Filter budgets by code (partial match)               |
| status     | List<int> | No       | Filter by status (1=Active, 2=Closed)                |
| isDeleted  | bool      | No       | Filter by deletion status                            |
| sortBy     | string    | No       | Sort field: "CreatedDate" (default), "Title", "Code" |
| sortOrder  | string    | No       | Sort order: "asc" or "desc" (default: "desc")        |
| pageNumber | int       | No       | Page number (default: 1)                             |
| pageSize   | int       | No       | Records per page (default: 10, max: 100)             |

#### Access: Manager, Employee (JWT Bearer)

**Description:** Role-based budget retrieval with intelligent filtering. Admins see all budgets, managers access budgets they created or manage, employees see budgets created by their manager. Provides comprehensive budget information with calculated metrics.

#### Status Codes

| Code | Status                | Message                        |
| ---- | --------------------- | ------------------------------ |
| 200  | OK                    | Budgets retrieved successfully |
| 400  | Bad Request           | Validation error               |
| 401  | Unauthorized          | Unauthorized                   |
| 403  | Forbidden             | Manager ID missing or invalid  |
| 500  | Internal Server Error | Failed to retrieve budgets     |

#### Response Body

**200 OK**
```json
{
    "data": [
        {
            "budgetID": 1,
            "title": "Engineering Operations",
            "code": "BT2601",
            "departmentID": 1,
            "departmentName": "Engineering Operations",
            "amountAllocated": 5000000.00,
            "amountSpent": 1068348.00,
            "amountRemaining": 3931652.00,
            "utilizationPercentage": 21.37,
            "startDate": "2026-02-25T01:24:20.3843916",
            "endDate": "2027-02-25T01:24:20.3843916",
            "status": 1,
            "statusName": "Active",
            "notes": null,
            "createdByUserID": 2,
            "createdByName": "Sanika Anil",
            "createdByEmployeeID": "MGR2601",
            "createdDate": "2026-02-25T01:24:20.3843916",
            "updatedDate": "2026-02-25T06:26:11.2566667",
            "daysRemaining": 365,
            "isExpired": false,
            "isOverBudget": false
        }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalRecords": 1,
    "totalPages": 1,
    "hasNextPage": false,
    "hasPreviousPage": false
}
```

---

### Create Budget

`POST http://localhost:5131/api/budgets`

#### Access: Manager only (JWT Bearer)

**Description:** Creates a new budget. The budget is automatically assigned to the authenticated user as creator. Enforces unique budget titles, validates amount fields, and creates audit trail. The Budget Code is auto-generated using the format `BT<YY><seq>` (e.g., `BT26001`) and cannot be provided by the client.

#### Request Body
```json
{
    "title": "Q1 2026 Operations",
    "departmentID": 1,
    "amountAllocated": 5000000.00,
    "startDate": "2026-02-25T00:00:00",
    "endDate": "2027-02-25T00:00:00",
    "notes": "Annual budget for engineering operations"
}
```

#### Status Codes

| Code | Status                | Message                 |
| ---- | --------------------- | ----------------------- |
| 201  | Created               | Budget is created       |
| 400  | Bad Request           | Validation error        |
| 401  | Unauthorized          | Unauthorized            |
| 403  | Forbidden             | Forbidden               |
| 409  | Conflict              | Title already exists    |
| 500  | Internal Server Error | Failed to create budget |

#### Response Body

**201 Created**
```json
{
    "success": true,
    "message": "Budget is created"
}
```

**409 Conflict (Duplicate Title)**
```json
{
    "success": false,
    "message": "Title already in use"
}
```

---

### Update Budget

`PUT http://localhost:5131/api/budgets/{budgetID}`

#### Route Parameters

| Parameter | Type | Required | Description        |
| --------- | ---- | -------- | ------------------ |
| budgetID  | int  | Yes      | Internal budget ID |

#### Access: Manager only (JWT Bearer)

**Description:** Updates an existing budget. Validates unique constraints for title, verifies no changes are made, and creates audit trail. The Budget Code is auto-generated and immutable — it cannot be modified after creation.

#### Request Body
```json
{
    "title": "Q1 2026 Operations Updated",
    "amountAllocated": 6000000.00,
    "startDate": "2026-02-25T00:00:00",
    "endDate": "2027-02-25T00:00:00",
    "notes": "Updated budget notes"
}
```

#### Status Codes

| Code | Status                | Message                  |
| ---- | --------------------- | ------------------------ |
| 200  | OK                    | Budget is updated        |
| 400  | Bad Request           | Validation or no changes |
| 401  | Unauthorized          | Unauthorized             |
| 403  | Forbidden             | Forbidden                |
| 404  | Not Found             | Budget not found         |
| 409  | Conflict              | Title already in use     |
| 500  | Internal Server Error | Failed to update budget  |

#### Response Body

**200 OK**
```json
{
    "success": true,
    "message": "Budget is updated"
}
```

**404 Not Found**
```json
{
    "success": false,
    "message": "Budget not found"
}
```

---

### Delete Budget

`DELETE http://localhost:5131/api/budgets/{budgetID}`

#### Route Parameters

| Parameter | Type | Required | Description        |
| --------- | ---- | -------- | ------------------ |
| budgetID  | int  | Yes      | Internal budget ID |

#### Access: Manager only (JWT Bearer)

**Description:** Soft deletes a budget by marking it as deleted. The record is preserved for audit purposes.

#### Status Codes

| Code | Status                | Message                 |
| ---- | --------------------- | ----------------------- |
| 200  | OK                    | Budget is deleted       |
| 401  | Unauthorized          | Unauthorized            |
| 403  | Forbidden             | Forbidden               |
| 404  | Not Found             | Budget not found        |
| 500  | Internal Server Error | Failed to delete budget |

#### Response Body

**200 OK**
```json
{
    "success": true,
    "message": "Budget is deleted"
}
```

---

### Get Expenses by Budget

`GET http://localhost:5131/api/budgets/{budgetID}/expenses`

#### Route Parameters

| Parameter | Type | Required | Description        |
| --------- | ---- | -------- | ------------------ |
| budgetID  | int  | Yes      | Internal budget ID |

#### Query Parameters

| Parameter             | Type   | Required | Description                                   |
| --------------------- | ------ | -------- | --------------------------------------------- |
| title                 | string | No       | Filter by expense title                       |
| status                | string | No       | Filter by status                              |
| categoryName          | string | No       | Filter by category name                       |
| submittedUserName     | string | No       | Filter by submitted user name                 |
| submittedByEmployeeID | string | No       | Filter by employee ID                         |
| sortBy                | string | No       | Sort field (default: "SubmittedDate")         |
| sortOrder             | string | No       | Sort order: "asc" or "desc" (default: "desc") |
| pageNumber            | int    | No       | Page number (default: 1)                      |
| pageSize              | int    | No       | Records per page (default: 10)                |

#### Access: Admin, Manager, Employee (JWT Bearer)

**Description:** Retrieves all expenses associated with a specific budget. Supports filtering, sorting, and pagination.

#### Status Codes

| Code | Status                | Message                 |
| ---- | --------------------- | ----------------------- |
| 200  | OK                    | Expenses retrieved      |
| 401  | Unauthorized          | Unauthorized            |
| 403  | Forbidden             | Forbidden               |
| 500  | Internal Server Error | Failed to retrieve data |

#### Response Body

**200 OK**
```json
{
    "data": [
        {
            "expenseID": 1,
            "budgetID": 1,
            "budgetTitle": "Engineering Operations",
            "budgetCode": "BT2601",
            "categoryID": 4,
            "categoryName": "Cloud Infrastructure",
            "title": "Monthly Cloud Hosting",
            "amount": 109913.00,
            "merchantName": "HashiCorp Inc",
            "status": 1,
            "statusName": "Pending",
            "submittedDate": "2026-01-28T03:56:07.919",
            "submittedByUserID": 7,
            "submittedByUserName": "Shivali Sharma",
            "submittedByEmployeeID": "EMP2601"
        }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalRecords": 5,
    "totalPages": 1,
    "hasNextPage": false,
    "hasPreviousPage": false
}
```

| Code | Status                | Message                        |
| ---- | --------------------- | ------------------------------ |
| 200  | OK                    | Budgets retrieved successfully |
| 401  | Unauthorized          | Role not found                 |
| 400  | Bad Request           | Manager ID missing             |
| 500  | Internal Server Error | Internal server error          |

### Response Body
```json
{
    "data": [
        {
            "budgetID": 1,
            "title": "Engineering Operations",
            "code": "BT2601",
            "departmentID": 1,
            "departmentName": "Engineering Operations",
            "amountAllocated": 5000000.00,
            "amountSpent": 1068348.00,
            "amountRemaining": 3931652.00,
            "utilizationPercentage": 21.3669600000000000,
            "startDate": "2026-02-25T01:24:20.3843916",
            "endDate": "2027-02-25T01:24:20.3843916",
            "status": 1,
            "statusName": "Active",
            "notes": null,
            "createdByUserID": 2,
            "createdByName": "Sanika Anil",
            "createdByEmployeeID": "MGR2601",
            "createdDate": "2026-02-25T01:24:20.3843916",
            "updatedDate": "2026-02-25T06:26:11.2566667",
            "daysRemaining": 365,
            "isExpired": false,
            "isOverBudget": false
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
*Note: Returns role-filtered budget data from vwGetAllBudgets view with calculated fields and user-based access control*

`POST http://localhost:5131/api/budgets`

Request Body
```json
{
    "title": "Marketing Q3",
    "code": "MKT004",
    "departmentID": 1,
    "amountAllocated": 10000,
    "startDate": "2026-01-01T09:00:00",
    "endDate": "2026-04-30T18:00:00",
    "notes": ""
}
```
### Access Manager only with Jwt bearer

**Note:** Creates a new budget record using the `uspCreateBudget` stored procedure with comprehensive validation and audit logging. The procedure performs duplicate checking for budget codes, initializes budget amounts (allocated, spent, remaining), and creates a detailed audit trail. Automatically generates JSON-formatted audit logs capturing the complete budget state for compliance and tracking purposes. Budgets are created with active status by default and include calculated fields for financial monitoring. Restricted to administrators and managers to maintain budget creation governance.

### Status Codes

| Code | Status       | Message                            |
| ---- | ------------ | ---------------------------------- |
| 201  | Created      | Budget is created                  |
| 400  | Bad Request  | Bad request (invalid input)        |
| 401  | Unauthorized | Unauthorized                       |
| 403  | Forbidden    | Forbidden                          |
| 409  | Conflict     | Conflict (duplicate title or code) |

### Response Body

**201 Created**
```json
{
    "success": true,
    "message": "Budget is created"
}
```
*Note: Returned when uspCreateBudget successfully inserts a new budget record with audit logging*

**409 Conflict**
```json
{
    "success": false,
    "message": "Title or code already exists"
}
```
*Note: Triggered by uspCreateBudget duplicate validation for existing active budget codes*

**400 Bad Request**
```json
{
    "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "errors": {
        "dto": [
            "The dto field is required."
        ],
        "$.departmentID": [
            "The JSON value could not be converted to System.Int32. Path: $.departmentID | LineNumber: 3 | BytePositionInLine: 27."
        ]
    },
    "traceId": "00-7ad91d1010b41dfaba5ca03a3854a031-3dc702851dd78dc1-00"
}
```

`PUT http://localhost:5131/api/budgets/{budgetID}`

Request Body
```json
{
    "title": "Updated Marketing Q3",
    "code": "MKT004",
    "departmentID": 1,
    "amountAllocated": 15000,
    "startDate": "2026-01-01T09:00:00",
    "endDate": "2026-04-30T18:00:00",
    "status": 1,
    "notes": "Updated notes"
}
```
### Access Manager only with Jwt bearer

**Note:** Updates an existing budget using the `uspUpdateBudget` stored procedure with intelligent change detection and comprehensive audit logging. The procedure captures before/after states as JSON for complete audit trails, performs duplicate validation (excluding the current budget), and includes a no-change detection mechanism to prevent unnecessary updates. Automatically recalculates remaining amounts when allocation changes and maintains data integrity through transaction-based operations. Supports partial updates while preserving audit compliance and allowing budget creators or administrators to modify budget details.

### Status Codes

| Code | Status       | Message                                   |
| ---- | ------------ | ----------------------------------------- |
| 200  | OK           | Budget is updated                         |
| 400  | Bad Request  | Bad request (invalid input or no changes) |
| 401  | Unauthorized | Unauthorized                              |
| 403  | Forbidden    | Forbidden                                 |
| 404  | Not Found    | Not found (budget not found)              |
| 409  | Conflict     | Conflict (duplicate title or code)        |

### Response Body

**200 OK**
```json
{
    "success" : true,
    "message": "Budget is updated"
}
```
*Note: Returned when uspUpdateBudget successfully updates budget with audit logging of before/after states*

**400 Bad Request (No changes)**
```json
{
    "success": false,
    "message": "No changes made"
}
```
*Note: Triggered by uspUpdateBudget change detection when input matches existing data exactly*

**404 Not Found**
```json
{
    "success": false,
    "message": "Budget not found"
}
```
*Note: Returned when uspUpdateBudget cannot find active budget with specified ID*

**409 Conflict**
```json
{
    "success": false,
    "message": "Title or code already exists"
}
```
*Note: Triggered by uspUpdateBudget duplicate validation for title/code conflicts with other budgets*

**400 Bad Request (Validation errors)**
```json
{
    "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
    "title": "One or more validation errors occurred.",
    "status": 400,
    "errors": {
        "dto": [
            "The dto field is required."
        ],
        "$.departmentID": [
            "The JSON value could not be converted to System.Int32. Path: $.departmentID | LineNumber: 3 | BytePositionInLine: 27."
        ]
    },
    "traceId": "00-7ad91d1010b41dfaba5ca03a3854a031-3dc702851dd78dc1-00"
}
```

`DELETE http://localhost:5131/api/budgets/{budgetID}`

### Access Manager only with Jwt bearer

**Note:** Performs a soft delete operation using the `uspDeleteBudget` stored procedure, marking budgets as deleted rather than physically removing them. This preserves data integrity and audit trails while preventing access to deleted budgets in standard queries. The procedure automatically sets the budget status to 'Closed', records deletion metadata (user and timestamp), and creates comprehensive audit logs with pre-deletion state capture. Deleted budgets remain accessible through administrative views for compliance and historical reporting purposes, ensuring no financial data is permanently lost.

### Status Codes

| Code | Status       | Message                      |
| ---- | ------------ | ---------------------------- |
| 200  | OK           | Budget is deleted            |
| 401  | Unauthorized | Unauthorized                 |
| 403  | Forbidden    | Forbidden                    |
| 404  | Not Found    | Not found (budget not found) |

### Response Body

**200 OK**
```json
{
    "success": true,
    "message": "Budget is deleted"
}
```
*Note: Returned when uspDeleteBudget successfully performs soft delete with audit logging*

**404 Not Found**
```json
{
    "success": false,
    "message": "Budget not found"
}
```
*Note: Triggered when uspDeleteBudget cannot find active budget or budget already deleted*