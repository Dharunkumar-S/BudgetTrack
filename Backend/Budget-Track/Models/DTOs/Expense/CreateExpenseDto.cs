#nullable enable
using System.ComponentModel.DataAnnotations;

namespace Budget_Track.Models.DTOs.Expense
{
    public class CreateExpenseDTO
    {
        [Required]
        public int BudgetId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [MaxLength(200)]
        public string? MerchantName { get; set; }
    }
}
