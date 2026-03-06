#nullable enable
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Budget_Track.Models.DTOs.Reports
{
    /// <summary>
    /// Department summary for Department Report
    /// </summary>
    [Keyless]
    public class DepartmentSummaryDto
    {
        [JsonPropertyName("departmentCode")]
        public string DepartmentCode { get; set; } = string.Empty;

        [JsonPropertyName("departmentName")]
        public string DepartmentName { get; set; } = string.Empty;

        [JsonPropertyName("amountAllocated")]
        public decimal AmountAllocated { get; set; }

        [JsonPropertyName("amountSpent")]
        public decimal AmountSpent { get; set; }

        [JsonPropertyName("amountRemaining")]
        public decimal AmountRemaining { get; set; }

        [JsonPropertyName("utilizationPercentage")]
        public decimal UtilizationPercentage { get; set; }

        [JsonPropertyName("budgetcount")]
        public int BudgetCount { get; set; }

        [JsonPropertyName("expensecount")]
        public int ExpenseCount { get; set; }
    }
}
