#nullable enable
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Budget_Track.Models.DTOs.Reports
{
    /// <summary>
    /// Response DTO for Department Report
    /// </summary>
    [Keyless]
    public class DepartmentReportDto
    {
        [JsonPropertyName("totalbudgetAmount")]
        public decimal TotalBudgetAmount { get; set; }

        [JsonPropertyName("totalbudgetAmountUsed")]
        public decimal TotalBudgetAmountUsed { get; set; }

        [JsonPropertyName("totalbudgetAmountRemaining")]
        public decimal TotalBudgetAmountRemaining { get; set; }

        [JsonPropertyName("totalbudgetUtilizationPercentage")]
        public decimal TotalBudgetUtilizationPercentage { get; set; }

        [JsonPropertyName("totalDepartmentcount")]
        public int TotalDepartmentCount { get; set; }

        [NotMapped]
        [JsonPropertyName("departments")]
        public List<DepartmentSummaryDto> Departments { get; set; } = new();
    }
}
