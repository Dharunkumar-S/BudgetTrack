#nullable enable
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Budget_Track.Models.DTOs.Reports
{
    /// <summary>
    /// Response DTO for Period Report
    /// </summary>
    [Keyless]
    public class PeriodReportDto
    {
        [JsonPropertyName("startDate")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("endDate")]
        public DateTime EndDate { get; set; }

        [JsonPropertyName("totalBudgetCount")]
        public int TotalBudgetCount { get; set; }

        [JsonPropertyName("totalBudgetAmount")]
        public decimal TotalBudgetAmount { get; set; }

        [JsonPropertyName("totalBudgetAmountSpent")]
        public decimal TotalBudgetAmountSpent { get; set; }

        [JsonPropertyName("totalBudgetAmountRemaining")]
        public decimal TotalBudgetAmountRemaining { get; set; }

        [JsonPropertyName("utilizationPercentage")]
        public decimal UtilizationPercentage { get; set; }

        [NotMapped]
        [JsonPropertyName("budgets")]
        public List<BudgetSummaryDto> Budgets { get; set; } = new();
    }
}
