#nullable enable
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Budget_Track.Models.DTOs.Reports
{
    /// <summary>
    /// Budget summary for Period Report
    /// </summary>
    [Keyless]
    public class BudgetSummaryDto
    {
        [JsonPropertyName("budgetCode")]
        public string BudgetCode { get; set; } = string.Empty;

        [JsonPropertyName("budgetTitle")]
        public string BudgetTitle { get; set; } = string.Empty;

        [JsonPropertyName("allocatedAmount")]
        public decimal AllocatedAmount { get; set; }

        [JsonPropertyName("amountSpent")]
        public decimal AmountSpent { get; set; }

        [JsonPropertyName("amountRemaining")]
        public decimal AmountRemaining { get; set; }

        [JsonPropertyName("utilizationPercentage")]
        public decimal UtilizationPercentage { get; set; }
    }
}
