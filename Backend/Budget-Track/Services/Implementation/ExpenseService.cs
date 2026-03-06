using Budget_Track.Models.DTOs.Expense;
using Budget_Track.Models.DTOs.Pagination;
using Budget_Track.Repositories.Interfaces;
using Budget_Track.Services.Interfaces;

namespace Budget_Track.Services.Implementation
{
    public class ExpenseService : IExpenseService
    {
        private readonly IExpenseRepository _repository;

        public ExpenseService(IExpenseRepository repository)
        {
            _repository = repository;
        }

        public async Task<PagedResult<AllExpenseDto>> GetAllExpensesAsync(ExpenseFilterDto filters)
        {
            return await _repository.GetAllExpensesAsync(filters);
        }

        public async Task<PagedResult<AllExpenseDto>> GetExpensesByBudgetIDAsync(
            int budgetID,
            ExpenseFilterDto filters
        )
        {
            return await _repository.GetExpensesByBudgetIDAsync(budgetID, filters);
        }

        public async Task<PagedResult<AllExpenseDto>> GetManagedExpensesAsync(
            ManagedExpenseFilterDto filters
        )
        {
            return await _repository.GetManagedExpensesAsync(filters);
        }

        public async Task<int> CreateExpenseAsync(CreateExpenseDTO dto, int submittedByUserID)
        {
            return await _repository.CreateExpenseAsync(dto, submittedByUserID);
        }

        public async Task<bool> UpdateExpenseStatusAsync(
            int expenseID,
            int status,
            int approvedByUserID,
            string? approvalComments = null,
            string? rejectionReason = null
        )
        {
            return await _repository.UpdateExpenseStatusAsync(
                expenseID,
                status,
                approvedByUserID,
                approvalComments,
                rejectionReason
            );
        }

        public async Task<ExpenseStatisticsDto> GetExpenseStatisticsAsync(
            ManagedExpenseFilterDto filters
        )
        {
            return await _repository.GetExpenseStatisticsAsync(filters);
        }
    }
}
