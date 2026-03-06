using Budget_Track.Models.DTOs.Budget;
using Budget_Track.Models.DTOs.Pagination;

namespace Budget_Track.Repositories.Interfaces
{
    public interface IBudgetRepository
    {
        Task<int> CreateBudgetAsync(CreateBudgetDto dto, int createdByUserID);
        Task UpdateBudgetAsync(int budgetID, UpdateBudgetDto dto, int updatedByUserID);
        Task DeleteBudgetAsync(int budgetID, int deletedByUserID);

        Task<BudgetDto?> GetBudgetByIdAsync(int budgetID);
        Task<PagedResult<BudgetDto>> GetAllBudgetsAsync(BudgetFilterDto filters);
        Task<PagedResult<BudgetDto>> GetBudgetsByCreatedByUserIdWithPaginationAsync(
            int createdByUserID,
            BudgetFilterDto filters
        );
    }
}