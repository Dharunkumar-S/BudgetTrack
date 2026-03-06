#nullable enable
using System.ComponentModel.DataAnnotations;
using Budget_Track.Models.Enums;

namespace Budget_Track.Models.DTOs.Budget
{
    public class UpdateBudgetStatusDto
    {
        [Required(ErrorMessage = "Status is required")]
        public BudgetStatus Status { get; set; }

        [MaxLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string? Notes { get; set; }

        [Required(ErrorMessage = "UpdatedByUserID is required")]
        public int UpdatedByUserID { get; set; }
    }
}
