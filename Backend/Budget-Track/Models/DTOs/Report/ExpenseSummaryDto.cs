#nullable enable
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Budget_Track.Models.DTOs.Reports
{
    /// <summary>
    /// Expense summary for Budget Report — includes SubmittedEmployeeId
    /// </summary>
    [Keyless]
    public class ExpenseSummaryDto
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("merchantName")]
        public string? MerchantName { get; set; }

        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("submittedBy")]
        public string SubmittedBy { get; set; } = string.Empty;

        [JsonPropertyName("submittedEmployeeId")]
        public string SubmittedEmployeeId { get; set; } = string.Empty;

        [JsonPropertyName("submittedDate")]
        public DateTime SubmittedDate { get; set; }
    }
}
