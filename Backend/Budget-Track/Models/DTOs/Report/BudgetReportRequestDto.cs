#nullable enable
using System.ComponentModel.DataAnnotations;

namespace Budget_Track.Models.DTOs.Reports
{
    /// <summary>
    /// Request DTO for Budget Report
    /// </summary>
    public class BudgetReportRequestDto
    {
        [Required(ErrorMessage = "Budget code is required")]
        public required string BudgetCode { get; set; }
    }
}
