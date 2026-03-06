#nullable enable
namespace Budget_Track.Models.DTOs.Expense
{
    public class ManagedExpenseFilterDto : ExpenseFilterDto
    {
        public bool MyExpensesOnly { get; set; } = false;
    }
}
