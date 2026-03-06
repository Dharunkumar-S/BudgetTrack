## Expense API

All routes are served from `http://localhost:5131/api/expenses` and require JWT Bearer authentication.

---

### Get Expense Statistics

`GET http://localhost:5131/api/expenses/stats`

#### Query Parameters

| Parameter             | Type   | Required | Description                                        |
| --------------------- | ------ | -------- | -------------------------------------------------- |
| budgetID              | int    | No       | Filter by budget ID                                |
| title                 | string | No       | Filter by expense title                            |
| budgetTitle           | string | No       | Filter by budget title                             |
| status                | string | No       | Filter by status (Pending, Approved, Rejected)     |
| categoryName          | string | No       | Filter by category name                            |
| submittedUserName     | string | No       | Filter by submitter name                           |
| submittedByEmployeeID | string | No       | Filter by employee ID                              |
| departmentName        | string | No       | Filter by department name                          |
| myExpensesOnly        | bool   | No       | Show only current user's expenses (default: false) |

#### Access: Admin, Manager, Employee (JWT Bearer)

**Description:** Retrieves comprehensive expense statistics including counts, totals, and summaries based on applied filters. Useful for dashboard and reporting purposes.

#### Status Codes

| Code | Status                | Message                  |
| ---- | --------------------- | ------------------------ |
| 200  | OK                    | Statistics retrieved     |
| 401  | Unauthorized          | Unauthorized             |
| 403  | Forbidden             | Forbidden                |
| 500  | Internal Server Error | Failed to retrieve stats |

#### Response Body

**200 OK**
```json
{
    "totalExpenses": 76,
    "pendingExpenses": 25,
    "approvedExpenses": 40,
    "rejectedExpenses": 11,
    "totalAmount": 5234567.89,
    "totalAmountPending": 1234567.89,
    "totalAmountApproved": 3500000.00,
    "totalAmountRejected": 500000.00
}
```

---

### Get All Expenses

`GET http://localhost:5131/api/expenses`

#### Query Parameters

| Parameter             | Type   | Required | Description                                                             |
| --------------------- | ------ | -------- | ----------------------------------------------------------------------- |
| title                 | string | No       | Filter by expense title (partial match)                                 |
| budgetTitle           | string | No       | Filter by budget title (partial match)                                  |
| status                | string | No       | Filter by status name                                                   |
| categoryName          | string | No       | Filter by category name (partial match)                                 |
| submittedUserName     | string | No       | Filter by submitter full name (partial)                                 |
| submittedByEmployeeID | string | No       | Filter by submitter employee ID                                         |
| departmentName        | string | No       | Filter by department name (partial match)                               |
| sortBy                | string | No       | Sort field: "SubmittedDate" (default), "Amount", "Title", "BudgetTitle" |
| sortOrder             | string | No       | Sort direction: "asc" or "desc" (default: "desc")                       |
| pageNumber            | int    | No       | Page number (default: 1)                                                |
| pageSize              | int    | No       | Records per page (default: 10, max: 100)                                |

#### Access: Admin, Manager, Employee (JWT Bearer)

**Description:** Retrieves all expenses across all budgets with comprehensive filtering, sorting, and pagination. Joins expense, budget, category, user, and department data.

#### Status Codes

| Code | Status                | Message                     |
| ---- | --------------------- | --------------------------- |
| 200  | OK                    | Expenses retrieved          |
| 401  | Unauthorized          | Unauthorized                |
| 403  | Forbidden             | Forbidden                   |
| 500  | Internal Server Error | Failed to retrieve expenses |

#### Response Body

**200 OK**
```json
{
    "data": [
        {
            "expenseID": 1,
            "budgetID": 2,
            "budgetTitle": "Engineering Operations",
            "budgetCode": "BUD2601",
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
            "submittedByEmployeeID": "EMP2601",
            "departmentName": "Engineering Operations",
            "approvedByUserID": null,
            "approvedByUserName": null,
            "approvedDate": null,
            "approvalComments": null,
            "rejectionReason": null,
            "notes": "Project X infrastructure"
        }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalRecords": 76,
    "totalPages": 8,
    "hasNextPage": true,
    "hasPreviousPage": false
}
```

---

### Get Managed Expenses

`GET http://localhost:5131/api/expenses/managed`

#### Query Parameters

| Parameter             | Type   | Required | Description                                        |
| --------------------- | ------ | -------- | -------------------------------------------------- |
| title                 | string | No       | Filter by expense title                            |
| status                | string | No       | Filter by status                                   |
| categoryName          | string | No       | Filter by category name                            |
| submittedUserName     | string | No       | Filter by submitter name                           |
| submittedByEmployeeID | string | No       | Filter by employee ID                              |
| myExpensesOnly        | bool   | No       | Show only current user's expenses (default: false) |
| sortBy                | string | No       | Sort field (default: "SubmittedDate")              |
| sortOrder             | string | No       | Sort direction: "asc" or "desc" (default: "desc")  |
| pageNumber            | int    | No       | Page number (default: 1)                           |
| pageSize              | int    | No       | Records per page (default: 10)                     |

#### Access: Manager, Employee (JWT Bearer)

**Description:** Retrieves expenses managed by the authenticated user. Managers see their team's expenses, employees see their own. Provides team-level and individual expense visibility.

#### Status Codes

| Code | Status                | Message                     |
| ---- | --------------------- | --------------------------- |
| 200  | OK                    | Expenses retrieved          |
| 401  | Unauthorized          | Unauthorized                |
| 403  | Forbidden             | Forbidden                   |
| 500  | Internal Server Error | Failed to retrieve expenses |

#### Response Body

**200 OK**
```json
{
    "data": [
        {
            "expenseID": 1,
            "title": "Monthly Cloud Hosting",
            "amount": 109913.00,
            "status": "Pending",
            "submittedDate": "2026-01-28T03:56:07.919",
            "submittedByUserName": "Shivali Sharma",
            "categoryName": "Cloud Infrastructure"
        }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalRecords": 15,
    "totalPages": 2,
    "hasNextPage": true,
    "hasPreviousPage": false
}
```

---

### Create Expense

`POST http://localhost:5131/api/expenses`

#### Access: Manager, Employee (JWT Bearer)

**Description:** Creates a new expense under a budget. Validates budget and category exist, checks budget has sufficient remaining amount, creates audit trail.

#### Request Body
```json
{
    "budgetID": 1,
    "categoryID": 4,
    "title": "Q1 2026 Cloud Services",
    "amount": 50000.00,
    "merchantName": "Cloud Provider Inc",
    "notes": "Monthly subscription"
}
```

#### Status Codes

| Code | Status                | Message                      |
| ---- | --------------------- | ---------------------------- |
| 201  | Created               | Expense is created           |
| 400  | Bad Request           | Validation or budget error   |
| 401  | Unauthorized          | Unauthorized                 |
| 403  | Forbidden             | Forbidden                    |
| 404  | Not Found             | Budget or category not found |
| 500  | Internal Server Error | Failed to create expense     |

#### Response Body

**201 Created**
```json
{
    "expenseId": 25,
    "message": "Expense is created"
}
```

**400 Bad Request (Insufficient Budget)**
```json
{
    "success": false,
    "message": "Insufficient budget remaining"
}
```

---

### Update Expense Status

`PUT http://localhost:5131/api/expenses/status/{expenseID}`

#### Route Parameters

| Parameter | Type | Required | Description         |
| --------- | ---- | -------- | ------------------- |
| expenseID | int  | Yes      | Internal expense ID |

#### Access: Manager (JWT Bearer)

**Description:** Updates the status of an expense (Approved or Rejected). Managers can approve or reject employee expense submissions. Rejection requires a reason, approval can include comments.

#### Request Body (Approval)
```json
{
    "status": 2,
    "comments": "Approved for processing"
}
```

#### Request Body (Rejection)
```json
{
    "status": 3,
    "reason": "Duplicate expense submission"
}
```

#### Status Codes

| Code | Status                | Message                      |
| ---- | --------------------- | ---------------------------- |
| 200  | OK                    | Expense is approved/rejected |
| 400  | Bad Request           | Validation or logic error    |
| 401  | Unauthorized          | Unauthorized                 |
| 403  | Forbidden             | Forbidden                    |
| 404  | Not Found             | Expense not found            |
| 500  | Internal Server Error | Failed to update status      |

#### Response Body

**200 OK (Approved)**
```json
{
    "success": true,
    "message": "Expense is approved"
}
```

**200 OK (Rejected)**
```json
{
    "success": true,
    "message": "Expense is rejected"
}
```

**404 Not Found**
```json
{
    "success": false,
    "message": "Expense not found"
}
```

### Query Parameters

| Parameter             | Type   | Required | Message                                                                 |
| --------------------- | ------ | -------- | ----------------------------------------------------------------------- |
| budgetID              | int    | Yes      | Route parameter identifying the parent budget.                          |
| status                | string | No       | Filter by status name (`Pending`, `Approved`, `Rejected`, `Cancelled`). |
| categoryName          | string | No       | Filter by category name (uses `LIKE '%value%'`).                        |
| submittedUserName     | string | No       | Filter by submitter full name (uses `LIKE '%value%'`).                  |
| submittedByEmployeeID | string | No       | Filter by submitter employee ID (exact match).                          |
| sortOrder             | string | No       | Sort direction for `submittedDate`. Accepts `asc` or `desc` (default).  |
| pageNumber            | int    | No       | Page number (default 1).                                                |
| pageSize              | int    | No       | Page size (default 10, capped at 100).                                  |

### Access Admin, Manager, Employee with Jwt bearer

**Note:** Uses the `vwGetExpensesByBudgetID` view to join expense, category, and user data. The repository applies dynamic `WHERE` clauses, ensures consistent ordering by `SubmittedDate`, and returns a `PagedResult<ExpenseDetailDto>` with audit-ready fields including approval comments and rejection reasons. All soft-deleted expenses are excluded at the view level.

### Status Codes

| Code | Status                | Message                         |
| ---- | --------------------- | ------------------------------- |
| 200  | OK                    | Expenses retrieved successfully |
| 401  | Unauthorized          | Missing or invalid JWT          |
| 403  | Forbidden             | Caller lacks the required role  |
| 500  | Internal Server Error | Failed to retrieve expenses     |

### Response Body

```json
{
    "data": [
        {
            "expenseID": 1,
            "budgetID": 1,
            "categoryID": 4,
            "categoryName": "Virtual Machine Hosting",
            "title": "Monthly Cloud Hosting",
            "amount": 109913.00,
            "merchantName": "HashiCorp Inc",
            "status": 1,
            "statusName": "Pending",
            "submittedDate": "2026-01-28T03:56:07.919",
            "submittedByUserID": 7,
            "submittedByUserName": "John Doe",
            "submittedByEmployeeID": "EMP001",
            "approvedByUserID": null,
            "approvedByUserName": null,
            "approvalComments": null,
            "rejectionReason": null,
            "notes": "Project X infrastructure"
        }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalRecords": 45,
    "totalPages": 5,
    "hasNextPage": true,
    "hasPreviousPage": false,
    "firstPage": 1,
    "lastPage": 5,
    "nextPage": 2,
    "previousPage": null,
    "firstItemIndex": 1,
    "lastItemIndex": 10,
    "currentPageItemCount": 10,
    "isFirstPage": true,
    "isLastPage": false
}
```
*Note: Pagination metadata is produced by `PagedResult<T>`; property names are serialized to camelCase by the API pipeline.*

`POST http://localhost:5131/api/expenses`

### Access Manager, Employee with Jwt bearer

**Request Body**

```json
{
    "budgetId": 1,
    "categoryId": 8,
    "title": "API Gateway Subscription",
    "amount": 94036.00,
    "merchantName": "Atlassian Pty Ltd"
}
```

**Note:** Calls `uspCreateExpense`, which validates the target budget and category, inserts the expense in `Pending (1)` status, emits a full-entity JSON snapshot to `tAuditLog`, and creates an `ExpenseApprovalReminder` (Type 1) notification for the submitter's manager. The stored procedure also enforces soft-delete checks on budgets, categories, and submitters.

### Status Codes

| Code | Status                | Message                                               |
| ---- | --------------------- | ----------------------------------------------------- |
| 201  | Created               | Expense is created                                    |
| 400  | Bad Request           | Validation failure / insufficient funds / not pending |
| 401  | Unauthorized          | Missing or invalid JWT                                |
| 403  | Forbidden             | Role not allowed to submit expenses                   |
| 404  | Not Found             | Budget or category not found / already deleted        |
| 500  | Internal Server Error | Unexpected server error                               |

### Response Body

**201 Created**

```json
{
    "expenseId": 101,
    "message": "Expense is created"
}
```
*Note: `NewExpenseID` is surfaced via the stored procedure output parameter.*

**400 Bad Request**

```json
{
    "success": false,
    "message": "Budget not found or has been deleted"
}
```
*Note: Domain errors such as missing budgets, exceeded allocations, or invalid state transitions are mapped to 400 responses; standard ASP.NET model validation errors emit the default RFC 9110 payload.*

`PUT http://localhost:5131/api/expenses/status/{expenseID}`

`http://localhost:5131/api/expenses/status/1`

### Access Manager with Jwt bearer

**Request Body (Approval)**

```json
{
    "status": 2,
    "comments": "Approved for Q1 budget."
}
```

**Request Body (Rejection)**

```json
{
    "status": 3,
    "reason": "Missing receipt documentation."
}
```

**Note:** Executes `uspUpdateExpenseStatus`, which enforces valid transitions (only `Approved (2)` or `Rejected (3)`), prevents redundant updates, and manages cascading side effects:
- Adjusts `tBudget.AmountSpent` / `AmountRemaining` when approvals or reversals occur. On approval, `AmountRemaining` is capped at `0` — never goes negative even if the expense exceeds the allocated budget.
- Captures `OldValue`/`NewValue` JSON snapshots for both the expense and impacted budget in `tAuditLog`.
- Sends `ExpenseApproved (Type 2)` or `ExpenseRejected (Type 3)` notifications to the submitting employee.
- Records approver comments (`ApprovalComments`) or rejection reasons.

### Status Codes

| Code | Status                | Message                                                      |
| ---- | --------------------- | ------------------------------------------------------------ |
| 200  | OK                    | Expense is approved / Expense is rejected                    |
| 400  | Bad Request           | No changes made / invalid status / expense cannot be updated |
| 401  | Unauthorized          | Missing or invalid JWT                                       |
| 403  | Forbidden             | Caller is not in the Manager role                            |
| 404  | Not Found             | Expense not found or has been deleted                        |
| 500  | Internal Server Error | Unexpected server error                                      |

### Response Body

**200 OK**

```json
{
    "success": true,
    "message": "Expense is approved"
}
```

**400 Bad Request (No Changes Detected)**

```json
{
    "success": false,
    "message": "No changes made"
}
```

**400 Bad Request (Invalid State)**

```json
{
    "success": false,
    "message": "Expense cannot be updated in its current state"
}
```
*Note: Attempting to set any value other than 2 or 3, or modifying a cancelled expense, causes the stored procedure to raise an error that the controller surfaces as a 400 response.*
 