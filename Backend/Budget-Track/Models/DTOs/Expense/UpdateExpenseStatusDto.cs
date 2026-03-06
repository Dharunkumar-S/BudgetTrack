#nullable enable
using System.ComponentModel.DataAnnotations;
using Budget_Track.Models.Enums;

namespace Budget_Track.Models.DTOs.Expense
{
    public class UpdateExpenseStatusDto
    {
        [Required]
        public ExpenseStatus Status { get; set; }

        [MaxLength(500)]
        public string? Comments { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }
    }
}
