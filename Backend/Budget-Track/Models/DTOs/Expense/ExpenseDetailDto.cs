#nullable enable
using Budget_Track.Models.Enums;

namespace Budget_Track.Models.DTOs.Expense
{
    public class ExpenseDetailDto
    {
        public int ExpenseID { get; set; }
        public int BudgetID { get; set; }
        public int CategoryID { get; set; }
        public required string CategoryName { get; set; }
        public required string Title { get; set; }
        public decimal Amount { get; set; }
        public string? MerchantName { get; set; }
        public int Status { get; set; }
        public required string StatusName { get; set; }
        public DateTime SubmittedDate { get; set; }
        public int SubmittedByUserID { get; set; }
        public required string SubmittedByUserName { get; set; }
        public required string SubmittedByEmployeeID { get; set; }
        public int? ManagerUserID { get; set; }
        public string? ApprovedByUserName { get; set; }
        public string? ApprovalComments { get; set; }
        public string? RejectionReason { get; set; }
        public string? Notes { get; set; }
    }
}
