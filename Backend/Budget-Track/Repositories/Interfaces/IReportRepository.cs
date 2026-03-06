using Budget_Track.Models.DTOs.Reports;
using Budget_Track.Models.Entities;
using Budget_Track.Models.Enums;

namespace Budget_Track.Repositories.Interfaces
{
    public interface IReportRepository
    {
        Task<PeriodReportDto> GetPeriodReportAsync(DateTime startDate, DateTime endDate);
        Task<DepartmentReportDto> GetDepartmentReportAsync(string? departmentName);
        Task<BudgetReportDto> GetBudgetReportAsync(string budgetCode);
    }
}
