#nullable enable
using System.ComponentModel.DataAnnotations;
using Budget_Track.Models.Enums;

namespace Budget_Track.Models.DTOs.Budget
{
    public class UpdateBudgetDto
    {
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string? Title { get; set; }

        public int? DepartmentID { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Amount Allocated must be greater than 0")]
        public decimal? AmountAllocated { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [MaxLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string? Notes { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public BudgetStatus Status { get; set; }
    }
}