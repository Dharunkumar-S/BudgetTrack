// File: Services/Interfaces/IReportService.cs
using Budget_Track.Models.DTOs.Reports;
using Budget_Track.Models.Entities;

namespace Budget_Track.Services.Interfaces
{
    public interface IReportService
    {
        Task<PeriodReportDto> GetPeriodReportAsync(DateTime startDate, DateTime endDate);
        Task<DepartmentReportDto> GetDepartmentReportAsync(string? departmentName);
        Task<BudgetReportDto> GetBudgetReportAsync(string budgetCode);
    }
}
