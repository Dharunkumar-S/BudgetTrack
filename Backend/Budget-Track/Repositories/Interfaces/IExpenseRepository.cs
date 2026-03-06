using Budget_Track.Models.DTOs.Expense;
using Budget_Track.Models.DTOs.Pagination;

namespace Budget_Track.Repositories.Interfaces
{
    public interface IExpenseRepository
    {
        Task<PagedResult<AllExpenseDto>> GetAllExpensesAsync(ExpenseFilterDto filters);

        Task<PagedResult<AllExpenseDto>> GetExpensesByBudgetIDAsync(
            int budgetID,
            ExpenseFilterDto filters
        );

        Task<PagedResult<AllExpenseDto>> GetManagedExpensesAsync(ManagedExpenseFilterDto filters);

        Task<int> CreateExpenseAsync(CreateExpenseDTO dto, int submittedByUserID);

        Task<bool> UpdateExpenseStatusAsync(
            int expenseID,
            int status,
            int approvedByUserID,
            string? approvalComments = null,
            string? rejectionReason = null
        );

        Task<ExpenseStatisticsDto> GetExpenseStatisticsAsync(ManagedExpenseFilterDto filters);
    }
}
