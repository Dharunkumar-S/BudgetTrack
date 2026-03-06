using System.Data;
using Budget_Track.Data;
using Budget_Track.Models.DTOs.Reports;
using Budget_Track.Models.Entities;
using Budget_Track.Models.Enums;
using Budget_Track.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Budget_Track.Repositories.Implementation
{
    public class ReportRepository : IReportRepository
    {
        private readonly BudgetTrackDbContext _context;

        public ReportRepository(BudgetTrackDbContext context)
        {
            _context = context;
        }

        public async Task<PeriodReportDto> GetPeriodReportAsync(
            DateTime startDate,
            DateTime endDate
        )
        {
            var budgets = await _context
                .Database.SqlQueryRaw<BudgetSummaryDto>(
                    "EXEC uspGetPeriodReport @StartDate, @EndDate",
                    new SqlParameter("@StartDate", startDate),
                    new SqlParameter("@EndDate", endDate)
                )
                .ToListAsync();

            var totalAllocated = budgets.Sum(b => b.AllocatedAmount);
            var totalSpent = budgets.Sum(b => b.AmountSpent);

            return new PeriodReportDto
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalBudgetCount = budgets.Count,
                TotalBudgetAmount = totalAllocated,
                TotalBudgetAmountSpent = totalSpent,
                TotalBudgetAmountRemaining = budgets.Sum(b => b.AmountRemaining),
                UtilizationPercentage =
                    totalAllocated > 0 ? (totalSpent / totalAllocated) * 100 : 0,
                Budgets = budgets,
            };
        }

        public async Task<DepartmentReportDto> GetDepartmentReportAsync(string? departmentName)
        {
            var departments = await _context
                .Database.SqlQueryRaw<DepartmentSummaryDto>(
                    "EXEC uspGetDepartmentReport @DepartmentName",
                    new SqlParameter("@DepartmentName", (object?)departmentName ?? DBNull.Value)
                )
                .ToListAsync();

            var totalAllocated = departments.Sum(d => d.AmountAllocated);
            var totalSpent = departments.Sum(d => d.AmountSpent);

            return new DepartmentReportDto
            {
                TotalBudgetAmount = totalAllocated,
                TotalBudgetAmountUsed = totalSpent,
                TotalBudgetAmountRemaining = departments.Sum(d => d.AmountRemaining),
                TotalBudgetUtilizationPercentage =
                    totalAllocated > 0 ? (totalSpent / totalAllocated) * 100 : 0,
                TotalDepartmentCount = departments.Count,
                Departments = departments,
            };
        }

        public async Task<BudgetReportDto> GetBudgetReportAsync(string budgetCode)
        {
            var budgetList = await _context
                .Database.SqlQueryRaw<BudgetReportDto>(
                    "EXEC uspGetBudgetReport @BudgetCode",
                    new SqlParameter("@BudgetCode", budgetCode)
                )
                .ToListAsync();

            var budget = budgetList.FirstOrDefault();

            if (budget == null)
            {
                throw new KeyNotFoundException($"Budget with code '{budgetCode}' not found");
            }

            // Get expense counts using stored procedure
            var countsList = await _context
                .Database.SqlQueryRaw<ExpenseCountDto>(
                    "EXEC uspGetBudgetReportExpenseCounts @BudgetCode",
                    new SqlParameter("@BudgetCode", budgetCode)
                )
                .ToListAsync();

            var counts = countsList.FirstOrDefault();

            budget.TotalExpenseCount = counts?.TotalExpenseCount ?? 0;
            budget.PendingExpenseCount = counts?.PendingExpenseCount ?? 0;
            budget.ApprovedExpenseCount = counts?.ApprovedExpenseCount ?? 0;
            budget.RejectedExpenseCount = counts?.RejectedExpenseCount ?? 0;

            budget.ApprovalRate =
                budget.TotalExpenseCount > 0
                    ? ((decimal)budget.ApprovedExpenseCount / (decimal)budget.TotalExpenseCount)
                        * 100
                    : 0;

            var expenses = await _context
                .Database.SqlQueryRaw<ExpenseSummaryDto>(
                    "EXEC uspGetBudgetReportExpenses @BudgetCode",
                    new SqlParameter("@BudgetCode", budgetCode)
                )
                .ToListAsync();

            budget.Expenses = expenses;

            return budget;
        }

        private class ExpenseCountDto
        {
            public int TotalExpenseCount { get; set; }
            public int PendingExpenseCount { get; set; }
            public int ApprovedExpenseCount { get; set; }
            public int RejectedExpenseCount { get; set; }
        }
    }
}
