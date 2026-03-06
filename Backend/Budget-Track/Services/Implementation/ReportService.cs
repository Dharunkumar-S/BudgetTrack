using Budget_Track.Data;
using Budget_Track.Models.DTOs.Reports;
using Budget_Track.Models.Entities;
using Budget_Track.Models.Enums;
using Budget_Track.Repositories.Interfaces;
using Budget_Track.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Budget_Track.Services.Implementation
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _repository;

        public ReportService(IReportRepository repository)
        {
            _repository = repository;
        }

        public async Task<PeriodReportDto> GetPeriodReportAsync(
            DateTime startDate,
            DateTime endDate
        )
        {
            if (startDate > endDate)
            {
                throw new ArgumentException("Start date must be before end date");
            }

            return await _repository.GetPeriodReportAsync(startDate, endDate);
        }

        public async Task<DepartmentReportDto> GetDepartmentReportAsync(string? departmentName)
        {
            return await _repository.GetDepartmentReportAsync(departmentName);
        }

        public async Task<BudgetReportDto> GetBudgetReportAsync(string budgetCode)
        {
            if (string.IsNullOrWhiteSpace(budgetCode))
            {
                throw new ArgumentException("Budget code is required");
            }

            return await _repository.GetBudgetReportAsync(budgetCode);
        }
    }
}
