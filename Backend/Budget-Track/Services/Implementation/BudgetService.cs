using Budget_Track.Models.DTOs.Budget;
using Budget_Track.Models.DTOs.Pagination;
using Budget_Track.Models.Enums;
using Budget_Track.Repositories.Interfaces;
using Budget_Track.Services.Interfaces;

namespace Budget_Track.Services.Implementation
{
    public class BudgetService : IBudgetService
    {
        private readonly IBudgetRepository _budgetRepository;

        public BudgetService(IBudgetRepository budgetRepository)
        {
            _budgetRepository = budgetRepository;
        }

        public async Task<int> CreateBudgetAsync(CreateBudgetDto dto, int createdByUserID)
        {
            if (dto.StartDate >= dto.EndDate)
            {
                throw new ArgumentException("Start Date must be before End Date");
            }

            if (dto.AmountAllocated <= 0)
            {
                throw new ArgumentException("Amount Allocated must be greater than 0");
            }

            if (dto.DepartmentID <= 0)
            {
                throw new ArgumentException("Department is required");
            }

            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                throw new ArgumentException("Title is required");
            }

            return await _budgetRepository.CreateBudgetAsync(dto, createdByUserID);
        }

        public async Task UpdateBudgetAsync(int budgetID, UpdateBudgetDto dto, int updatedByUserID)
        {
            // Check if budget exists
            var budget = await _budgetRepository.GetBudgetByIdAsync(budgetID);
            if (budget == null)
            {
                throw new InvalidOperationException("Budget not found");
            }

            // For managers, check if they own the budget (created by them)
            // Check is done here - if CreatedByUserID doesn't match, access is denied
            // Admins bypass this check (handled by controller role authorization)
            if (budget.CreatedByUserID != updatedByUserID)
            {
                throw new UnauthorizedAccessException("You can only update budgets you created");
            }

            // Validate only if fields are provided
            if (dto.StartDate.HasValue && dto.EndDate.HasValue && dto.StartDate >= dto.EndDate)
            {
                throw new ArgumentException("Start Date must be before End Date");
            }

            if (dto.AmountAllocated.HasValue && dto.AmountAllocated <= 0)
            {
                throw new ArgumentException("Amount Allocated must be greater than 0");
            }

            // Pass the DTO directly to repository - the stored procedure will handle NULL values
            await _budgetRepository.UpdateBudgetAsync(budgetID, dto, updatedByUserID);
        }

        public async Task DeleteBudgetAsync(int budgetID, int deletedByUserID)
        {
            await _budgetRepository.DeleteBudgetAsync(budgetID, deletedByUserID);
        }

        public async Task<BudgetDto?> GetBudgetByIdAsync(int budgetID)
        {
            return await _budgetRepository.GetBudgetByIdAsync(budgetID);
        }

        public async Task<PagedResult<BudgetDto>> GetAllBudgetsAsync(BudgetFilterDto filters)
        {
            return await _budgetRepository.GetAllBudgetsAsync(filters);
        }

        public async Task<PagedResult<BudgetDto>> GetBudgetsByCreatedByUserIdWithPaginationAsync(
            int createdByUserID,
            BudgetFilterDto filters
        )
        {
            return await _budgetRepository.GetBudgetsByCreatedByUserIdWithPaginationAsync(
                createdByUserID,
                filters
            );
        }
    }
}