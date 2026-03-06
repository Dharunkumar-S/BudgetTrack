USE [Budget-Track];
GO

-- Get Period Report
CREATE OR ALTER PROCEDURE uspGetPeriodReport
    @StartDate DATETIME,
    @EndDate DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Code AS BudgetCode,
        Title AS BudgetTitle,
        AmountAllocated AS AllocatedAmount,
        (SELECT ISNULL(SUM(e.Amount), 0)
        FROM tExpense e
        WHERE e.BudgetID = tBudget.BudgetID
            AND e.IsDeleted = 0
            AND e.Status = 2) AS AmountSpent,
        CASE 
            WHEN (SELECT ISNULL(SUM(e.Amount), 0)
        FROM tExpense e
        WHERE e.BudgetID = tBudget.BudgetID
            AND e.IsDeleted = 0
            AND e.Status = 2) > AmountAllocated THEN 0
            ELSE AmountAllocated - (SELECT ISNULL(SUM(e.Amount), 0)
        FROM tExpense e
        WHERE e.BudgetID = tBudget.BudgetID
            AND e.IsDeleted = 0
            AND e.Status = 2)
        END AS AmountRemaining,
        CASE WHEN AmountAllocated > 0 
            THEN ((SELECT ISNULL(SUM(e.Amount), 0)
        FROM tExpense e
        WHERE e.BudgetID = tBudget.BudgetID
            AND e.IsDeleted = 0
            AND e.Status = 2) / AmountAllocated) * 100 
            ELSE 0 END AS UtilizationPercentage
    FROM tBudget
    WHERE IsDeleted = 0
        AND StartDate >= @StartDate
        AND EndDate <= @EndDate
    ORDER BY Code;
END
GO

-- Get Department Report
CREATE OR ALTER PROCEDURE uspGetDepartmentReport
    @DepartmentName NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        d.DepartmentCode AS DepartmentCode,
        d.DepartmentName AS DepartmentName,
        ISNULL(SUM(b.AmountAllocated), 0) AS AmountAllocated,
        ISNULL(SUM(CASE WHEN e.Status = 2 THEN e.Amount ELSE 0 END), 0) AS AmountSpent,
        CASE 
            WHEN ISNULL(SUM(CASE WHEN e.Status = 2 THEN e.Amount ELSE 0 END), 0) > ISNULL(SUM(b.AmountAllocated), 0) THEN 0
            ELSE ISNULL(SUM(b.AmountAllocated), 0) - ISNULL(SUM(CASE WHEN e.Status = 2 THEN e.Amount ELSE 0 END), 0)
        END AS AmountRemaining,
        CASE WHEN ISNULL(SUM(b.AmountAllocated), 0) > 0 
            THEN (ISNULL(SUM(CASE WHEN e.Status = 2 THEN e.Amount ELSE 0 END), 0) / SUM(b.AmountAllocated)) * 100 
            ELSE 0 END AS UtilizationPercentage,
        COUNT(DISTINCT b.BudgetID) AS BudgetCount,
        COUNT(DISTINCT e.ExpenseID) AS ExpenseCount
    FROM tDepartment d
        LEFT JOIN tBudget b ON d.DepartmentID = b.DepartmentID AND b.IsDeleted = 0
        LEFT JOIN tExpense e ON b.BudgetID = e.BudgetID AND e.IsDeleted = 0
    WHERE d.IsDeleted = 0
        AND (@DepartmentName IS NULL OR d.DepartmentName = @DepartmentName)
    GROUP BY d.DepartmentCode, d.DepartmentName
    ORDER BY d.DepartmentName;
END
GO
GO

-- Get Budget Report (now includes Department and Manager info)
CREATE OR ALTER PROCEDURE uspGetBudgetReport
    @BudgetCode NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        b.Code AS BudgetCode,
        b.Title AS BudgetTitle,
        d.DepartmentName AS DepartmentName,
        u.FirstName + ' ' + u.LastName AS ManagerName,
        u.EmployeeID AS ManagerEmployeeId,
        b.AmountAllocated AS AllocatedAmount,
        (SELECT ISNULL(SUM(e.Amount), 0)
        FROM tExpense e
        WHERE e.BudgetID = b.BudgetID
            AND e.IsDeleted = 0
            AND e.Status = 2) AS AmountSpent,
        CASE 
            WHEN (SELECT ISNULL(SUM(e.Amount), 0)
        FROM tExpense e
        WHERE e.BudgetID = b.BudgetID
            AND e.IsDeleted = 0
            AND e.Status = 2) > b.AmountAllocated THEN 0
            ELSE b.AmountAllocated - (SELECT ISNULL(SUM(e.Amount), 0)
        FROM tExpense e
        WHERE e.BudgetID = b.BudgetID
            AND e.IsDeleted = 0
            AND e.Status = 2)
        END AS AmountRemaining,
        b.StartDate,
        b.EndDate,
        DATEDIFF(DAY, GETUTCDATE(), b.EndDate) AS DaysRemaining,
        CASE b.Status
            WHEN 1 THEN 'Active'
            WHEN 2 THEN 'Closed'
            ELSE 'Unknown'
        END AS Status,
        CASE WHEN b.EndDate < GETUTCDATE() THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsExpired,
        CASE WHEN b.AmountAllocated > 0 
            THEN ((SELECT ISNULL(SUM(e.Amount), 0)
        FROM tExpense e
        WHERE e.BudgetID = b.BudgetID
            AND e.IsDeleted = 0
            AND e.Status = 2) / b.AmountAllocated) * 100 
            ELSE 0 END AS UtilizationPercentage,
        0 AS TotalExpenseCount,
        0 AS PendingExpenseCount,
        0 AS ApprovedExpenseCount,
        0 AS RejectedExpenseCount,
        CAST(0 AS DECIMAL(18,2)) AS ApprovalRate
    FROM tBudget b
        INNER JOIN tDepartment d ON b.DepartmentID = d.DepartmentID
        INNER JOIN tUser u ON b.CreatedByUserID = u.UserID
    WHERE b.IsDeleted = 0 AND b.Code = @BudgetCode;
END
GO

-- Get Budget Report Expense Counts
CREATE OR ALTER PROCEDURE uspGetBudgetReportExpenseCounts
    @BudgetCode NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        ISNULL(COUNT(e.ExpenseID), 0) AS TotalExpenseCount,
        ISNULL(SUM(CASE WHEN e.Status = 1 THEN 1 ELSE 0 END), 0) AS PendingExpenseCount,
        ISNULL(SUM(CASE WHEN e.Status = 2 THEN 1 ELSE 0 END), 0) AS ApprovedExpenseCount,
        ISNULL(SUM(CASE WHEN e.Status = 3 THEN 1 ELSE 0 END), 0) AS RejectedExpenseCount
    FROM tBudget b
        LEFT JOIN tExpense e ON b.BudgetID = e.BudgetID AND e.IsDeleted = 0
    WHERE b.IsDeleted = 0 AND b.Code = @BudgetCode;
END
GO

-- Get Budget Report Expenses (now includes SubmittedEmployeeId)
CREATE OR ALTER PROCEDURE uspGetBudgetReportExpenses
    @BudgetCode NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        e.Title,
        e.MerchantName,
        c.CategoryName AS Category,
        e.Amount,
        CASE e.Status
            WHEN 1 THEN 'Pending'
            WHEN 2 THEN 'Approved'
            WHEN 3 THEN 'Rejected'
            WHEN 4 THEN 'Cancelled'
            ELSE 'Unknown'
        END AS Status,
        u.FirstName + ' ' + u.LastName AS SubmittedBy,
        u.EmployeeID AS SubmittedEmployeeId,
        e.SubmittedDate
    FROM tExpense e
        INNER JOIN tBudget b ON e.BudgetID = b.BudgetID
        INNER JOIN tCategory c ON e.CategoryID = c.CategoryID
        INNER JOIN tUser u ON e.SubmittedByUserID = u.UserID
    WHERE e.IsDeleted = 0 AND b.Code = @BudgetCode
    ORDER BY e.SubmittedDate DESC;
END
GO