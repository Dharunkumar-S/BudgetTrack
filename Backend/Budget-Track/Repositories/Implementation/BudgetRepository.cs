using Budget_Track.Data;
using Budget_Track.Models.DTOs.Budget;
using Budget_Track.Models.DTOs.Pagination;
using Budget_Track.Models.Enums;
using Budget_Track.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Budget_Track.Repositories.Implementation
{
    public class BudgetRepository : IBudgetRepository
    {
        private readonly BudgetTrackDbContext _context;

        public BudgetRepository(BudgetTrackDbContext context)
        {
            _context = context;
        }

        public async Task<int> CreateBudgetAsync(CreateBudgetDto dto, int createdByUserID)
        {
            var budgetIDParam = new SqlParameter
            {
                ParameterName = "@BudgetID",
                SqlDbType = System.Data.SqlDbType.Int,
                Direction = System.Data.ParameterDirection.Output,
            };

            await _context.Database.ExecuteSqlRawAsync(
                @"EXEC dbo.uspCreateBudget 
                    @Title, @DepartmentID, @AmountAllocated, @StartDate, @EndDate, 
                    @Status, @Notes, @CreatedByUserID, @BudgetID OUTPUT",
                new SqlParameter("@Title", dto.Title),
                new SqlParameter("@DepartmentID", dto.DepartmentID),
                new SqlParameter("@AmountAllocated", dto.AmountAllocated),
                new SqlParameter("@StartDate", dto.StartDate),
                new SqlParameter("@EndDate", dto.EndDate),
                new SqlParameter("@Status", (int)BudgetStatus.Active),
                new SqlParameter("@Notes", (object?)dto.Notes ?? DBNull.Value),
                new SqlParameter("@CreatedByUserID", createdByUserID),
                budgetIDParam
            );

            return (int)budgetIDParam.Value;
        }

        public async Task UpdateBudgetAsync(int budgetID, UpdateBudgetDto dto, int updatedByUserID)
        {
            await _context.Database.ExecuteSqlRawAsync(
                @"EXEC dbo.uspUpdateBudget 
                    @BudgetID, @Title, @DepartmentID, @AmountAllocated, 
                    @StartDate, @EndDate, @Status, @Notes, @UpdatedByUserID",
                new SqlParameter("@BudgetID", budgetID),
                new SqlParameter("@Title", (object?)dto.Title ?? DBNull.Value),
                new SqlParameter("@DepartmentID", (object?)dto.DepartmentID ?? DBNull.Value),
                new SqlParameter("@AmountAllocated", (object?)dto.AmountAllocated ?? DBNull.Value),
                new SqlParameter("@StartDate", (object?)dto.StartDate ?? DBNull.Value),
                new SqlParameter("@EndDate", (object?)dto.EndDate ?? DBNull.Value),
                new SqlParameter("@Status", (int)dto.Status),
                new SqlParameter("@Notes", (object?)dto.Notes ?? DBNull.Value),
                new SqlParameter("@UpdatedByUserID", updatedByUserID)
            );
        }

        public async Task DeleteBudgetAsync(int budgetID, int deletedByUserID)
        {
            await _context.Database.ExecuteSqlRawAsync(
                @"EXEC dbo.uspDeleteBudget @BudgetID, @DeletedByUserID",
                new SqlParameter("@BudgetID", budgetID),
                new SqlParameter("@DeletedByUserID", deletedByUserID)
            );
        }

        public async Task<BudgetDto?> GetBudgetByIdAsync(int budgetID)
        {
            var results = await _context.Database
                .SqlQueryRaw<BudgetDto>(
                    "SELECT * FROM dbo.vwGetAllBudgetsAdmin WHERE BudgetID = @BudgetID AND IsDeleted = 0",
                    new SqlParameter("@BudgetID", budgetID))
                .ToListAsync();
            return results.FirstOrDefault();
        }

        public async Task<PagedResult<BudgetDto>> GetAllBudgetsAsync(BudgetFilterDto filters)
        {
            var pageNumber = filters.PageNumber ?? 1;
            var pageSize = Math.Min(filters.PageSize ?? 10, 100); // Max 100 records per page

            var sql = "SELECT * FROM dbo.vwGetAllBudgetsAdmin WHERE 1=1";
            var countSql = "SELECT COUNT(*) AS Value FROM dbo.vwGetAllBudgetsAdmin WHERE 1=1";
            var parameters = new List<SqlParameter>();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(filters.Title))
            {
                sql += " AND Title LIKE '%' + @Title + '%'";
                countSql += " AND Title LIKE '%' + @Title + '%'";
                parameters.Add(new SqlParameter("@Title", filters.Title));
            }

            if (!string.IsNullOrWhiteSpace(filters.Code))
            {
                sql += " AND Code LIKE '%' + @Code + '%'";
                countSql += " AND Code LIKE '%' + @Code + '%'";
                parameters.Add(new SqlParameter("@Code", filters.Code));
            }

            if (filters.Status != null && filters.Status.Count > 0)
            {
                var statusList = string.Join(",", filters.Status.Select((s, i) => $"@Status{i}"));
                sql += $" AND Status IN ({statusList})";
                countSql += $" AND Status IN ({statusList})";
                for (int i = 0; i < filters.Status.Count; i++)
                    parameters.Add(new SqlParameter($"@Status{i}", filters.Status[i]));
            }

            if (filters.IsDeleted.HasValue)
            {
                sql += " AND IsDeleted = @IsDeleted";
                countSql += " AND IsDeleted = @IsDeleted";
                parameters.Add(new SqlParameter("@IsDeleted", filters.IsDeleted.Value));
            }

            // Get total count
            var countParameters = parameters.Select(p => new SqlParameter(p.ParameterName, p.Value)).ToArray();
            var totalRecords = await _context.Database.SqlQueryRaw<int>(countSql, countParameters).ToListAsync();
            var total = totalRecords.FirstOrDefault();

            // Apply sorting
            var sortBy = filters.SortBy?.ToLower() ?? "createddate";
            var sortOrder = filters.SortOrder?.ToLower() == "asc" ? "ASC" : "DESC";

            sql += sortBy switch
            {
                "title" => $" ORDER BY Title {sortOrder}",
                "code" => $" ORDER BY Code {sortOrder}",
                _ => $" ORDER BY CreatedDate {sortOrder}",
            };

            // Apply pagination
            var offset = (pageNumber - 1) * pageSize;
            sql += $" OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY";

            var budgets = await _context.Database.SqlQueryRaw<BudgetDto>(sql, parameters.ToArray()).ToListAsync();

            return PagedResult<BudgetDto>.Create(budgets, pageNumber, pageSize, total);
        }

        public async Task<PagedResult<BudgetDto>> GetBudgetsByCreatedByUserIdWithPaginationAsync(
            int createdByUserID,
            BudgetFilterDto filters
        )
        {
            var pageNumber = filters.PageNumber ?? 1;
            var pageSize = Math.Min(filters.PageSize ?? 10, 100); // Max 100 records per page

            var sql = "SELECT * FROM dbo.vwGetAllBudgets WHERE 1=1 AND CreatedByUserID = @CreatedByUserID";
            var countSql = "SELECT COUNT(*) AS Value FROM dbo.vwGetAllBudgets WHERE 1=1 AND CreatedByUserID = @CreatedByUserID";
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@CreatedByUserID", createdByUserID),
            };

            // Apply filters
            if (!string.IsNullOrWhiteSpace(filters.Title))
            {
                sql += " AND Title LIKE '%' + @Title + '%'";
                countSql += " AND Title LIKE '%' + @Title + '%'";
                parameters.Add(new SqlParameter("@Title", filters.Title));
            }

            if (!string.IsNullOrWhiteSpace(filters.Code))
            {
                sql += " AND Code LIKE '%' + @Code + '%'";
                countSql += " AND Code LIKE '%' + @Code + '%'";
                parameters.Add(new SqlParameter("@Code", filters.Code));
            }

            if (filters.Status != null && filters.Status.Count > 0)
            {
                var statusList = string.Join(",", filters.Status.Select((s, i) => $"@Status{i}"));
                sql += $" AND Status IN ({statusList})";
                countSql += $" AND Status IN ({statusList})";
                for (int i = 0; i < filters.Status.Count; i++)
                    parameters.Add(new SqlParameter($"@Status{i}", filters.Status[i]));
            }

            // Get total count
            var countParameters = parameters.Select(p => new SqlParameter(p.ParameterName, p.Value)).ToArray();
            var totalRecords = await _context.Database.SqlQueryRaw<int>(countSql, countParameters).ToListAsync();
            var total = totalRecords.FirstOrDefault();

            // Apply sorting
            var sortBy = filters.SortBy?.ToLower() ?? "createddate";
            var sortOrder = filters.SortOrder?.ToLower() == "asc" ? "ASC" : "DESC";

            sql += sortBy switch
            {
                "title" => $" ORDER BY Title {sortOrder}",
                "code" => $" ORDER BY Code {sortOrder}",
                _ => $" ORDER BY CreatedDate {sortOrder}",
            };

            // Apply pagination
            var offset = (pageNumber - 1) * pageSize;
            sql += $" OFFSET {offset} ROWS FETCH NEXT {pageSize} ROWS ONLY";

            var budgets = await _context.Database.SqlQueryRaw<BudgetDto>(sql, parameters.ToArray()).ToListAsync();

            return PagedResult<BudgetDto>.Create(budgets, pageNumber, pageSize, total);
        }
    }
}