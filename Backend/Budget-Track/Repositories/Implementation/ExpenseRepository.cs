using Budget_Track.Data;
using Budget_Track.Models.DTOs.Expense;
using Budget_Track.Models.DTOs.Pagination;
using Budget_Track.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Budget_Track.Repositories.Implementation
{
    public class ExpenseRepository : IExpenseRepository
    {
        private readonly BudgetTrackDbContext _context;

        public ExpenseRepository(BudgetTrackDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<AllExpenseDto>> GetAllExpensesAsync(ExpenseFilterDto filters)
        {
            var pageNumber = filters.PageNumber ?? 1;
            var pageSize = Math.Min(filters.PageSize ?? 10, 100);

            var sql = "SELECT * FROM dbo.vwGetAllExpenses WHERE 1=1";
            var countSql = "SELECT COUNT(*) AS Value FROM dbo.vwGetAllExpenses WHERE 1=1";
            var parameters = new List<SqlParameter>();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(filters.Title))
            {
                sql += " AND Title LIKE '%' + @Title + '%'";
                countSql += " AND Title LIKE '%' + @Title + '%'";
                parameters.Add(new SqlParameter("@Title", filters.Title));
            }

            if (!string.IsNullOrWhiteSpace(filters.BudgetTitle))
            {
                sql += " AND BudgetTitle LIKE '%' + @BudgetTitle + '%'";
                countSql += " AND BudgetTitle LIKE '%' + @BudgetTitle + '%'";
                parameters.Add(new SqlParameter("@BudgetTitle", filters.BudgetTitle));
            }

            if (!string.IsNullOrWhiteSpace(filters.Status))
            {
                sql += " AND StatusName = @Status";
                countSql += " AND StatusName = @Status";
                parameters.Add(new SqlParameter("@Status", filters.Status));
            }

            if (!string.IsNullOrWhiteSpace(filters.CategoryName))
            {
                sql += " AND CategoryName LIKE '%' + @CategoryName + '%'";
                countSql += " AND CategoryName LIKE '%' + @CategoryName + '%'";
                parameters.Add(new SqlParameter("@CategoryName", filters.CategoryName));
            }

            if (!string.IsNullOrWhiteSpace(filters.SubmittedUserName))
            {
                sql += " AND SubmittedByUserName LIKE '%' + @SubmittedUserName + '%'";
                countSql += " AND SubmittedByUserName LIKE '%' + @SubmittedUserName + '%'";
                parameters.Add(new SqlParameter("@SubmittedUserName", filters.SubmittedUserName));
            }

            if (!string.IsNullOrWhiteSpace(filters.SubmittedByEmployeeID))
            {
                sql += " AND SubmittedByEmployeeID = @SubmittedByEmployeeID";
                countSql += " AND SubmittedByEmployeeID = @SubmittedByEmployeeID";
                parameters.Add(
                    new SqlParameter("@SubmittedByEmployeeID", filters.SubmittedByEmployeeID)
                );
            }

            if (!string.IsNullOrWhiteSpace(filters.DepartmentName))
            {
                sql += " AND DepartmentName LIKE '%' + @DepartmentName + '%'";
                countSql += " AND DepartmentName LIKE '%' + @DepartmentName + '%'";
                parameters.Add(new SqlParameter("@DepartmentName", filters.DepartmentName));
            }

            // Get total count
            var countParameters = parameters
                .Select(p => new SqlParameter(p.ParameterName, p.Value))
                .ToArray();
            var totalRecords = await _context
                .Database.SqlQueryRaw<int>(countSql, countParameters)
                .ToListAsync();
            var total = totalRecords.FirstOrDefault();

            // Apply sorting
            var sortBy = filters.SortBy?.ToLower() ?? "submitteddate";
            var sortOrder = filters.SortOrder?.ToLower() == "asc" ? "ASC" : "DESC";

            sql += sortBy switch
            {
                "amount" => $" ORDER BY Amount {sortOrder}",
                "title" => $" ORDER BY Title {sortOrder}",
                "budgettitle" => $" ORDER BY BudgetTitle {sortOrder}",
                _ => $" ORDER BY SubmittedDate {sortOrder}",
            };

            // Apply pagination
            var offset = (pageNumber - 1) * pageSize;
            sql += $" OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY";

            // Get paginated data
            var expenses = await _context
                .Database.SqlQueryRaw<AllExpenseDto>(sql, parameters.ToArray())
                .ToListAsync();

            return PagedResult<AllExpenseDto>.Create(expenses, pageNumber, pageSize, total);
        }

        public async Task<PagedResult<AllExpenseDto>> GetExpensesByBudgetIDAsync(
            int budgetID,
            ExpenseFilterDto filters
        )
        {
            var pageNumber = filters.PageNumber ?? 1;
            var pageSize = Math.Min(filters.PageSize ?? 10, 100);

            var sql = "SELECT * FROM dbo.vwGetAllExpenses WHERE BudgetID = @BudgetID";
            var countSql =
                "SELECT COUNT(*) AS Value FROM dbo.vwGetAllExpenses WHERE BudgetID = @BudgetID";
            var parameters = new List<SqlParameter> { new SqlParameter("@BudgetID", budgetID) };

            if (!string.IsNullOrWhiteSpace(filters.Title))
            {
                sql += " AND Title LIKE '%' + @Title + '%'";
                countSql += " AND Title LIKE '%' + @Title + '%'";
                parameters.Add(new SqlParameter("@Title", filters.Title));
            }

            if (!string.IsNullOrWhiteSpace(filters.Status))
            {
                sql += " AND StatusName = @Status";
                countSql += " AND StatusName = @Status";
                parameters.Add(new SqlParameter("@Status", filters.Status));
            }

            if (!string.IsNullOrWhiteSpace(filters.CategoryName))
            {
                sql += " AND CategoryName LIKE '%' + @CategoryName + '%'";
                countSql += " AND CategoryName LIKE '%' + @CategoryName + '%'";
                parameters.Add(new SqlParameter("@CategoryName", filters.CategoryName));
            }

            if (!string.IsNullOrWhiteSpace(filters.SubmittedUserName))
            {
                sql += " AND SubmittedByUserName LIKE '%' + @SubmittedUserName + '%'";
                countSql += " AND SubmittedByUserName LIKE '%' + @SubmittedUserName + '%'";
                parameters.Add(new SqlParameter("@SubmittedUserName", filters.SubmittedUserName));
            }

            if (!string.IsNullOrWhiteSpace(filters.SubmittedByEmployeeID))
            {
                sql += " AND SubmittedByEmployeeID = @SubmittedByEmployeeID";
                countSql += " AND SubmittedByEmployeeID = @SubmittedByEmployeeID";
                parameters.Add(
                    new SqlParameter("@SubmittedByEmployeeID", filters.SubmittedByEmployeeID)
                );
            }

            // Get total count
            var countParameters = parameters
                .Select(p => new SqlParameter(p.ParameterName, p.Value))
                .ToArray();
            var totalRecords = await _context
                .Database.SqlQueryRaw<int>(countSql, countParameters)
                .ToListAsync();
            var total = totalRecords.FirstOrDefault();

            // Apply sorting
            var sortBy = filters.SortBy?.ToLower() ?? "submitteddate";
            var sortOrder = filters.SortOrder?.ToLower() == "asc" ? "ASC" : "DESC";

            sql += sortBy switch
            {
                "amount" => $" ORDER BY Amount {sortOrder}",
                "title" => $" ORDER BY Title {sortOrder}",
                _ => $" ORDER BY SubmittedDate {sortOrder}",
            };

            // Apply pagination
            var offset = (pageNumber - 1) * pageSize;
            sql += $" OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY";

            var expenses = await _context
                .Database.SqlQueryRaw<AllExpenseDto>(sql, parameters.ToArray())
                .ToListAsync();

            return PagedResult<AllExpenseDto>.Create(expenses, pageNumber, pageSize, total);
        }

        public async Task<PagedResult<AllExpenseDto>> GetManagedExpensesAsync(
            ManagedExpenseFilterDto filters
        )
        {
            var pageNumber = filters.PageNumber ?? 1;
            var pageSize = Math.Min(filters.PageSize ?? 10, 100);

            // Using DISTINCT to ensure unique expenses even if joins might cause duplication
            var sql =
                "SELECT DISTINCT v.* FROM dbo.vwGetAllExpenses v JOIN dbo.tUser u ON v.SubmittedByUserID = u.UserID JOIN dbo.tBudget b ON v.BudgetID = b.BudgetID WHERE 1=1";
            var countSql =
                "SELECT COUNT(DISTINCT v.ExpenseID) AS Value FROM dbo.vwGetAllExpenses v JOIN dbo.tUser u ON v.SubmittedByUserID = u.UserID JOIN dbo.tBudget b ON v.BudgetID = b.BudgetID WHERE 1=1";
            var parameters = new List<SqlParameter>();

            // Role-based filtering logic
            if (filters.MyExpensesOnly)
            {
                sql += " AND v.SubmittedByUserID = @CurrentUserId";
                countSql += " AND v.SubmittedByUserID = @CurrentUserId";
                parameters.Add(new SqlParameter("@CurrentUserId", filters.CurrentUserId));
            }
            else if (filters.Role?.Equals("Manager", StringComparison.OrdinalIgnoreCase) == true)
            {
                sql += " AND u.ManagerID = @CurrentUserId";
                countSql += " AND u.ManagerID = @CurrentUserId";
                parameters.Add(new SqlParameter("@CurrentUserId", filters.CurrentUserId));
            }
            else if (filters.Role?.Equals("Employee", StringComparison.OrdinalIgnoreCase) == true)
            {
                if (filters.ManagerId.HasValue)
                {
                    // Budgets created by their Manager AND Expenses submitted by subordinates of that Manager
                    sql += " AND b.CreatedByUserID = @ManagerId AND u.ManagerID = @ManagerId";
                    countSql += " AND b.CreatedByUserID = @ManagerId AND u.ManagerID = @ManagerId";
                    parameters.Add(new SqlParameter("@ManagerId", filters.ManagerId.Value));
                }
                else
                {
                    // Fallback to only seeing their own if no manager (shouldn't happen in valid org)
                    sql += " AND v.SubmittedByUserID = @CurrentUserId";
                    countSql += " AND v.SubmittedByUserID = @CurrentUserId";
                    parameters.Add(new SqlParameter("@CurrentUserId", filters.CurrentUserId));
                }
            }

            // Standard Filters
            if (!string.IsNullOrWhiteSpace(filters.Title))
            {
                sql += " AND v.Title LIKE '%' + @Title + '%'";
                countSql += " AND v.Title LIKE '%' + @Title + '%'";
                parameters.Add(new SqlParameter("@Title", filters.Title));
            }

            if (!string.IsNullOrWhiteSpace(filters.Status))
            {
                sql += " AND v.StatusName = @Status";
                countSql += " AND v.StatusName = @Status";
                parameters.Add(new SqlParameter("@Status", filters.Status));
            }

            if (!string.IsNullOrWhiteSpace(filters.CategoryName))
            {
                sql += " AND v.CategoryName LIKE '%' + @CategoryName + '%'";
                countSql += " AND v.CategoryName LIKE '%' + @CategoryName + '%'";
                parameters.Add(new SqlParameter("@CategoryName", filters.CategoryName));
            }

            if (!string.IsNullOrWhiteSpace(filters.SubmittedUserName))
            {
                sql += " AND v.SubmittedByUserName LIKE '%' + @SubmittedUserName + '%'";
                countSql += " AND v.SubmittedByUserName LIKE '%' + @SubmittedUserName + '%'";
                parameters.Add(new SqlParameter("@SubmittedUserName", filters.SubmittedUserName));
            }

            if (!string.IsNullOrWhiteSpace(filters.SubmittedByEmployeeID))
            {
                sql += " AND v.SubmittedByEmployeeID = @SubmittedByEmployeeID";
                countSql += " AND v.SubmittedByEmployeeID = @SubmittedByEmployeeID";
                parameters.Add(
                    new SqlParameter("@SubmittedByEmployeeID", filters.SubmittedByEmployeeID)
                );
            }

            // Get total count
            var countParameters = parameters
                .Select(p => new SqlParameter(p.ParameterName, p.Value))
                .ToArray();
            var totalRecords = await _context
                .Database.SqlQueryRaw<int>(countSql, countParameters)
                .ToListAsync();
            var total = totalRecords.FirstOrDefault();

            // Sorting
            var sortOrder = filters.SortOrder?.ToLower() == "asc" ? "ASC" : "DESC";
            sql += $" ORDER BY v.SubmittedDate {sortOrder}";

            // Pagination
            var offset = (pageNumber - 1) * pageSize;
            sql += $" OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY";

            var expenses = await _context
                .Database.SqlQueryRaw<AllExpenseDto>(sql, parameters.ToArray())
                .ToListAsync();

            return PagedResult<AllExpenseDto>.Create(expenses, pageNumber, pageSize, total);
        }

        public async Task<int> CreateExpenseAsync(CreateExpenseDTO dto, int submittedByUserID)
        {
            var expenseIDParam = new SqlParameter
            {
                ParameterName = "@NewExpenseID",
                SqlDbType = System.Data.SqlDbType.Int,
                Direction = System.Data.ParameterDirection.Output,
            };

            await _context.Database.ExecuteSqlRawAsync(
                @"EXEC uspCreateExpense 
                    @BudgetID, @CategoryID, @Title, @Amount, @MerchantName, @SubmittedByUserID, @Notes, @NewExpenseID OUTPUT",
                new SqlParameter("@BudgetID", dto.BudgetId),
                new SqlParameter("@CategoryID", dto.CategoryId),
                new SqlParameter("@Title", dto.Title),
                new SqlParameter("@Amount", dto.Amount),
                new SqlParameter("@MerchantName", (object?)dto.MerchantName ?? DBNull.Value),
                new SqlParameter("@SubmittedByUserID", submittedByUserID),
                new SqlParameter("@Notes", DBNull.Value),
                expenseIDParam
            );

            return (int)expenseIDParam.Value;
        }

        public async Task<bool> UpdateExpenseStatusAsync(
            int expenseID,
            int status,
            int approvedByUserID,
            string? approvalComments = null,
            string? rejectionReason = null
        )
        {
            await _context.Database.ExecuteSqlRawAsync(
                @"EXEC uspUpdateExpenseStatus 
                    @ExpenseID, @Status, @ManagerUserID, @ApprovalComments, @RejectionReason",
                new SqlParameter("@ExpenseID", expenseID),
                new SqlParameter("@Status", status),
                new SqlParameter("@ManagerUserID", approvedByUserID),
                new SqlParameter("@ApprovalComments", (object?)approvalComments ?? DBNull.Value),
                new SqlParameter("@RejectionReason", (object?)rejectionReason ?? DBNull.Value)
            );

            return true;
        }

        public async Task<ExpenseStatisticsDto> GetExpenseStatisticsAsync(
            ManagedExpenseFilterDto filters
        )
        {
            var sqlPrefix =
                "SELECT v.* FROM dbo.vwGetAllExpenses v JOIN dbo.tBudget b ON v.BudgetID = b.BudgetID JOIN dbo.tUser u ON v.SubmittedByUserID = u.UserID ";
            var sqlWhere = "WHERE 1=1";
            var parameters = new List<SqlParameter>();

            // 1. Budget Scoped Filter
            if (filters.BudgetID.HasValue)
            {
                sqlWhere += " AND v.BudgetID = @BudgetID";
                parameters.Add(new SqlParameter("@BudgetID", filters.BudgetID.Value));
            }

            // 2. Role-based/MyExpenses filtering (Matching GetManagedExpensesAsync logic)
            if (filters.MyExpensesOnly)
            {
                sqlWhere += " AND v.SubmittedByUserID = @CurrentUserId";
                parameters.Add(new SqlParameter("@CurrentUserId", filters.CurrentUserId));
            }
            else if (
                string.IsNullOrEmpty(filters.BudgetID.ToString()) && !filters.BudgetID.HasValue
            ) // Only apply global role rules if not budget-scoped
            {
                if (filters.Role?.Equals("Manager", StringComparison.OrdinalIgnoreCase) == true)
                {
                    sqlWhere += " AND u.ManagerID = @CurrentUserId";
                    parameters.Add(new SqlParameter("@CurrentUserId", filters.CurrentUserId));
                }
                else if (
                    filters.Role?.Equals("Employee", StringComparison.OrdinalIgnoreCase) == true
                )
                {
                    if (filters.ManagerId.HasValue)
                    {
                        sqlWhere +=
                            " AND b.CreatedByUserID = @ManagerId AND u.ManagerID = @ManagerId";
                        parameters.Add(new SqlParameter("@ManagerId", filters.ManagerId.Value));
                    }
                    else
                    {
                        sqlWhere += " AND v.SubmittedByUserID = @CurrentUserId";
                        parameters.Add(new SqlParameter("@CurrentUserId", filters.CurrentUserId));
                    }
                }
            }

            // 3. Search Filters (Matching both GetAllExpensesAsync and GetManagedExpensesAsync)
            if (!string.IsNullOrWhiteSpace(filters.Title))
            {
                sqlWhere += " AND v.Title LIKE '%' + @Title + '%'";
                parameters.Add(new SqlParameter("@Title", filters.Title));
            }

            if (!string.IsNullOrWhiteSpace(filters.BudgetTitle))
            {
                sqlWhere += " AND v.BudgetTitle LIKE '%' + @BudgetTitle + '%'";
                parameters.Add(new SqlParameter("@BudgetTitle", filters.BudgetTitle));
            }

            if (!string.IsNullOrWhiteSpace(filters.CategoryName))
            {
                sqlWhere += " AND v.CategoryName LIKE '%' + @CategoryName + '%'";
                parameters.Add(new SqlParameter("@CategoryName", filters.CategoryName));
            }

            if (!string.IsNullOrWhiteSpace(filters.SubmittedUserName))
            {
                sqlWhere += " AND v.SubmittedByUserName LIKE '%' + @SubmittedUserName + '%'";
                parameters.Add(new SqlParameter("@SubmittedUserName", filters.SubmittedUserName));
            }

            if (!string.IsNullOrWhiteSpace(filters.SubmittedByEmployeeID))
            {
                sqlWhere += " AND v.SubmittedByEmployeeID = @SubmittedByEmployeeID";
                parameters.Add(
                    new SqlParameter("@SubmittedByEmployeeID", filters.SubmittedByEmployeeID)
                );
            }

            if (!string.IsNullOrWhiteSpace(filters.DepartmentName))
            {
                sqlWhere += " AND v.DepartmentName LIKE '%' + @DepartmentName + '%'";
                parameters.Add(new SqlParameter("@DepartmentName", filters.DepartmentName));
            }

            var countSql =
                $@"
                SELECT 
                    COUNT(*) AS TotalCount,
                    ISNULL(SUM(CASE WHEN Status = 1 THEN 1 ELSE 0 END), 0) AS PendingCount,
                    ISNULL(SUM(CASE WHEN Status = 2 THEN 1 ELSE 0 END), 0) AS ApprovedCount,
                    ISNULL(SUM(CASE WHEN Status = 3 THEN 1 ELSE 0 END), 0) AS RejectedCount,
                    ISNULL(SUM(Amount), 0) AS TotalAmount
                FROM ({sqlPrefix} {sqlWhere}) AS StatsSource";

            var result = await _context
                .Database.SqlQueryRaw<ExpenseStatisticsDto>(countSql, parameters.ToArray())
                .ToListAsync();

            return result.FirstOrDefault() ?? new ExpenseStatisticsDto();
        }
    }
}