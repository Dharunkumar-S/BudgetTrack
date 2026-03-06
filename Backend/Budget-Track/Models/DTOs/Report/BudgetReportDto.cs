#nullable enable
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Budget_Track.Models.DTOs.Reports
{
    /// <summary>
    /// Response DTO for Budget Report — includes Department and Manager info
    /// </summary>
    [Keyless]
    public class BudgetReportDto
    {
        [JsonPropertyName("budgetCode")]
        public string BudgetCode { get; set; } = string.Empty;

        [JsonPropertyName("budgetTitle")]
        public string BudgetTitle { get; set; } = string.Empty;

        [JsonPropertyName("departmentName")]
        public string DepartmentName { get; set; } = string.Empty;

        [JsonPropertyName("managerName")]
        public string ManagerName { get; set; } = string.Empty;

        [JsonPropertyName("managerEmployeeId")]
        public string ManagerEmployeeId { get; set; } = string.Empty;

        [JsonPropertyName("allocatedAmount")]
        public decimal AllocatedAmount { get; set; }

        [JsonPropertyName("amountSpent")]
        public decimal AmountSpent { get; set; }

        [JsonPropertyName("amountRemaining")]
        public decimal AmountRemaining { get; set; }

        [JsonPropertyName("startDate")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("endDate")]
        public DateTime EndDate { get; set; }

        [JsonPropertyName("daysRemaining")]
        public int DaysRemaining { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("isExpired")]
        public bool IsExpired { get; set; }

        [JsonPropertyName("utilizationPercentage")]
        public decimal UtilizationPercentage { get; set; }

        [JsonPropertyName("totalExpenseCount")]
        public int TotalExpenseCount { get; set; }

        [JsonPropertyName("pendingExpenseCount")]
        public int PendingExpenseCount { get; set; }

        [JsonPropertyName("approvedExpenseCount")]
        public int ApprovedExpenseCount { get; set; }

        [JsonPropertyName("rejectedExpenseCount")]
        public int RejectedExpenseCount { get; set; }

        [JsonPropertyName("approvalRate")]
        public decimal ApprovalRate { get; set; }

        [NotMapped]
        [JsonPropertyName("expenses")]
        public List<ExpenseSummaryDto> Expenses { get; set; } = new();
    }
}
