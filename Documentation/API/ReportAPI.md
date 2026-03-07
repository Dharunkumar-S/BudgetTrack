## Report API

All routes are served from `http://localhost:5131/api/reports` and require JWT Bearer authentication. Access is role-restricted per endpoint.

---

### Get Period Report

`GET http://localhost:5131/api/reports/period`

#### Query Parameters

| Parameter | Type     | Required | Description                    |
| --------- | -------- | -------- | ------------------------------ |
| startDate | datetime | Yes      | Report start date (ISO format) |
| endDate   | datetime | Yes      | Report end date (ISO format)   |

#### Access: Admin only (JWT Bearer)

**Description:** Retrieves budget and expense summaries for a specified date range. Useful for period-based financial analysis and forecasting.

#### Status Codes

| Code | Status                | Message                   |
| ---- | --------------------- | ------------------------- |
| 200  | OK                    | Report retrieved          |
| 400  | Bad Request           | Invalid date parameters   |
| 401  | Unauthorized          | Unauthorized              |
| 403  | Forbidden             | Forbidden                 |
| 500  | Internal Server Error | Failed to generate report |

#### Response Body

**200 OK**
```json
{
    "startDate": "2026-01-01",
    "endDate": "2026-03-31",
    "totalBudgetAllocated": 10000000.00,
    "totalBudgetSpent": 2500000.00,
    "totalBudgetRemaining": 7500000.00,
    "utilizationPercentage": 25.00,
    "totalExpenses": 45,
    "approvedExpenses": 35,
    "pendingExpenses": 8,
    "rejectedExpenses": 2,
    "averageExpenseAmount": 55555.56
}
```

**400 Bad Request**
```json
{
    "success": false,
    "message": "End date must be after start date"
}
```

---

### Get Department Report

`GET http://localhost:5131/api/reports/department`

#### Access: Admin only (JWT Bearer)

**Description:** Retrieves budget and expense statistics grouped by department. Provides department-level financial overview.

#### Status Codes

| Code | Status                | Message                   |
| ---- | --------------------- | ------------------------- |
| 200  | OK                    | Report retrieved          |
| 401  | Unauthorized          | Unauthorized              |
| 403  | Forbidden             | Forbidden                 |
| 500  | Internal Server Error | Failed to generate report |

#### Response Body

**200 OK**
```json
{
    "departments": [
        {
            "departmentID": 1,
            "departmentName": "Engineering Operations",
            "totalBudgetAllocated": 5000000.00,
            "totalBudgetSpent": 1068348.00,
            "totalBudgetRemaining": 3931652.00,
            "utilizationPercentage": 21.37,
            "expenseCount": 25,
            "approvedExpenseCount": 20,
            "pendingExpenseCount": 3,
            "rejectedExpenseCount": 2
        },
        {
            "departmentID": 2,
            "departmentName": "Finance",
            "totalBudgetAllocated": 3000000.00,
            "totalBudgetSpent": 900000.00,
            "totalBudgetRemaining": 2100000.00,
            "utilizationPercentage": 30.00,
            "expenseCount": 15,
            "approvedExpenseCount": 12,
            "pendingExpenseCount": 2,
            "rejectedExpenseCount": 1
        }
    ]
}
```

---

### Get Budget Report

`GET http://localhost:5131/api/reports/budget`

#### Query Parameters

| Parameter  | Type   | Required | Description |
| ---------- | ------ | -------- | ----------- |
| budgetCode | string | Yes      | Budget code |

#### Access: Admin, Manager (JWT Bearer)

**Description:** Retrieves detailed budget information with all associated expenses. Provides comprehensive budget-level financial view.

#### Status Codes

| Code | Status                | Message                   |
| ---- | --------------------- | ------------------------- |
| 200  | OK                    | Report retrieved          |
| 401  | Unauthorized          | Unauthorized              |
| 403  | Forbidden             | Forbidden                 |
| 404  | Not Found             | Budget not found          |
| 500  | Internal Server Error | Failed to generate report |

#### Response Body

**200 OK**
```json
{
    "budgetID": 1,
    "budgetCode": "BT2601",
    "budgetTitle": "Engineering Operations",
    "departmentName": "Engineering Operations",
    "amountAllocated": 5000000.00,
    "amountSpent": 1068348.00,
    "amountRemaining": 3931652.00,
    "utilizationPercentage": 21.37,
    "startDate": "2026-02-25",
    "endDate": "2027-02-25",
    "status": "Active",
    "createdByName": "Sanika Anil",
    "createdDate": "2026-02-25",
    "expenses": [
        {
            "expenseID": 1,
            "title": "Monthly Cloud Hosting",
            "amount": 109913.00,
            "status": "Approved",
            "categoryName": "Cloud Infrastructure",
            "submittedDate": "2026-01-28",
            "submittedByName": "Shivali Sharma"
        },
        {
            "expenseID": 2,
            "title": "Hardware Upgrade",
            "amount": 250000.00,
            "status": "Pending",
            "categoryName": "Hardware",
            "submittedDate": "2026-02-15",
            "submittedByName": "John Doe"
        }
    ]
}
```

**404 Not Found**
```json
{
    "success": false,
    "message": "Budget not found"
}
```

`GET http://localhost:5131/api/reports/period`

`http://localhost:5131/api/reports/period?startDate=2026-02-01T00:00:00&endDate=2026-03-31T23:59:59`

### Query Parameters

| Parameter | Type     | Required | Message                             |
| --------- | -------- | -------- | ----------------------------------- |
| startDate | DateTime | Yes      | Start date for the reporting period |
| endDate   | DateTime | Yes      | End date for the reporting period   |

### Access Admin only with Jwt bearer

**Note:** Generates a comprehensive budget summary report for a specified date range using the `uspGetPeriodReport` stored procedure. This analytical endpoint retrieves all budgets that fall within the specified period (where StartDate >= @StartDate AND EndDate <= @EndDate) and calculates real-time financial metrics including allocated amounts, spent amounts, remaining balances, and utilization percentages. The procedure joins with tExpense to aggregate approved expenses (Status = 2) while excluding soft-deleted records. Essential for financial planning, period-end reporting, and budget performance analysis. Validates that the start date is before the end date to prevent logical errors. Restricted to administrators and managers for financial oversight.

### Status Codes

| Code | Status                | Message                            |
| ---- | --------------------- | ---------------------------------- |
| 200  | OK                    | Report generated successfully      |
| 400  | Bad Request           | Start date must be before end date |
| 401  | Unauthorized          | Unauthorized                       |
| 403  | Forbidden             | Forbidden                          |
| 500  | Internal Server Error | Failed to generate period report   |

### Response Body

**200 OK**
```json
{
    "startDate": "2026-02-01T00:00:00",
    "endDate": "2026-03-31T23:59:59",
    "totalBudgetCount": 2,
    "totalBudgetAmount": 5015000.00,
    "totalBudgetAmountSpent": 1073548.00,
    "totalBudgetAmountRemaining": 3941452.00,
    "utilizationPercentage": 21.41,
    "budgets": [
        {
            "budgetCode": "BT2601",
            "budgetTitle": "Engineering Operations",
            "allocatedAmount": 5000000.00,
            "amountSpent": 1068348.00,
            "amountRemaining": 3931652.00,
            "utilizationPercentage": 21.37
        },
        {
            "budgetCode": "MKT004",
            "budgetTitle": "Marketing Q3",
            "allocatedAmount": 15000.00,
            "amountSpent": 5200.00,
            "amountRemaining": 9800.00,
            "utilizationPercentage": 34.67
        }
    ]
}
```
*Note: Returned when uspGetPeriodReport successfully retrieves budgets within the date range. Repository calculates summary totals including totalBudgetCount, totalBudgetAmount, totalBudgetAmountSpent, totalBudgetAmountRemaining, and overall utilizationPercentage across all budgets in the period*

**400 Bad Request**
```json
{
    "success": false,
    "message": "Start date must be before end date"
}
```
*Note: Triggered by GetPeriodReportAsync service validation when startDate > endDate parameter*

---

`GET http://localhost:5131/api/reports/department`

`http://localhost:5131/api/reports/department`

### Access Admin only with Jwt bearer

**Note:** Generates comprehensive departmental budget and expense statistics using the `uspGetDepartmentReport` stored procedure. This aggregated reporting endpoint provides a high-level overview of financial allocation and utilization across all departments in the organization. The procedure performs complex aggregations joining tDepartment, tBudget, and tExpense tables to calculate total allocated amounts, spent amounts (approved expenses only with Status = 2), remaining balances, and utilization percentages for each department. Also includes budget and expense counts for volume metrics. Excludes soft-deleted records to ensure accurate current-state reporting. Critical for executive dashboards, departmental comparisons, and organizational budget oversight.

### Status Codes

| Code | Status                | Message                              |
| ---- | --------------------- | ------------------------------------ |
| 200  | OK                    | Report generated successfully        |
| 401  | Unauthorized          | Unauthorized                         |
| 403  | Forbidden             | Forbidden                            |
| 500  | Internal Server Error | Failed to generate department report |

### Response Body

**200 OK**
```json
{
    "totalbudgetAmount": 592500000.00,
    "totalbudgetAmountUsed": 3070460.00,
    "totalbudgetAmountRemaining": 589429540.00,
    "totalbudgetUtilizationPercentage": 0.5182,
    "totalDepartmentcount": 5,
    "departments": [
        {
            "departmentName": "AI & Machine Learning",
            "amountAllocated": 127500000.00,
            "amountSpent": 599376.00,
            "amountRemaining": 126900624.00,
            "utilizationPercentage": 0.47,
            "budgetcount": 1,
            "expensecount": 15
        },
        {
            "departmentName": "Engineering Operations",
            "amountAllocated": 75000000.00,
            "amountSpent": 447198.00,
            "amountRemaining": 74552802.00,
            "utilizationPercentage": 0.5962,
            "budgetcount": 1,
            "expensecount": 15
        }
    ]
}
```
*Note: Returned when uspGetDepartmentReport successfully aggregates departmental financial data. Repository calculates summary totals including totalbudgetAmount, totalbudgetAmountUsed, totalbudgetAmountRemaining, totalbudgetUtilizationPercentage, and totalDepartmentcount across all departments. Note the lowercase 'b' and 'c' in field names (budgetcount, expensecount) as defined in JsonPropertyName attributes*

---

`GET http://localhost:5131/api/reports/budget`

`http://localhost:5131/api/reports/budget?budgetCode=BT2601`

### Query Parameters

| Parameter  | Type   | Required | Message                                     |
| ---------- | ------ | -------- | ------------------------------------------- |
| budgetCode | string | Yes      | Budget code to generate detailed report for |

### Access Admin, Manager with Jwt bearer

**Note:** Generates a comprehensive detailed report for a specific budget using three stored procedures: `uspGetBudgetReport`, `uspGetBudgetReportExpenseCounts`, and `uspGetBudgetReportExpenses`. This multi-faceted endpoint provides complete budget analysis including financial summary, timeline information, expense statistics, and itemized expense details. The first procedure retrieves budget header information with calculated metrics (utilization, days remaining, expiration status). The second procedure aggregates expense counts by status (pending, approved, rejected) and calculates approval rates. The third procedure retrieves all associated expenses with category, submitter, and status details ordered by submission date. Essential for detailed budget monitoring, expense auditing, and budget performance analysis. Returns 404 if the budget code doesn't exist.

### Status Codes

| Code | Status                | Message                          |
| ---- | --------------------- | -------------------------------- |
| 200  | OK                    | Report generated successfully    |
| 400  | Bad Request           | Budget code is required          |
| 401  | Unauthorized          | Unauthorized                     |
| 403  | Forbidden             | Forbidden                        |
| 404  | Not Found             | Budget not found                 |
| 500  | Internal Server Error | Failed to generate budget report |

### Response Body

**200 OK**
```json
{
    "budgetCode": "BT2604",
    "budgetTitle": "Cybersecurity & Compliance",
    "allocatedAmount": 9500000.00,
    "amountSpent": 635837.00,
    "amountRemaining": 8864163.00,
    "startDate": "2026-02-25T09:02:29.478427",
    "endDate": "2027-02-25T09:02:29.478427",
    "daysRemaining": 365,
    "status": "Active",
    "isExpired": false,
    "utilizationPercentage": 6.693,
    "totalExpenseCount": 15,
    "PendingExpenseCount": 7,
    "ApprovedExpenseCount": 5,
    "RejectedExpenseCount": 3,
    "approvalRate": 33.333333333333336,
    "expenses": [
        {
            "Title": "Project Management Suite",
            "category": "Technical Training",
            "amount": 175586.00,
            "status": "Pending",
            "submittedBy": "Arjun K",
            "submittedDate": "2026-02-24T09:02:29.478427"
        },
        {
            "Title": "Threat Detection Software",
            "category": "Cloud Infrastructure",
            "amount": 88893.00,
            "status": "Pending",
            "submittedBy": "Arjun K",
            "submittedDate": "2026-02-19T09:02:29.478427"
        }
    ]
}
```
*Note: Returned when all three stored procedures (uspGetBudgetReport, uspGetBudgetReportExpenseCounts, uspGetBudgetReportExpenses) successfully execute and combine budget details with expense analytics. Note the capital 'P', 'A', 'R' in PendingExpenseCount, ApprovedExpenseCount, RejectedExpenseCount and capital 'T' in expense Title field as defined in JsonPropertyName attributes*

**404 Not Found**
```json
{
    "success": false,
    "message": "Budget not found"
}
```
*Note: Triggered when uspGetBudgetReport returns no results, indicating the budget code doesn't exist or is soft-deleted*

**400 Bad Request**
```json
{
    "success": false,
    "message": "Budget code is required"
}
```
*Note: Triggered by GetBudgetReportAsync service validation when budgetCode parameter is null or empty*
