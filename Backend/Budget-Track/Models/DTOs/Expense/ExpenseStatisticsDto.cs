namespace Budget_Track.Models.DTOs.Expense
{
    public class ExpenseStatisticsDto
    {
        public int TotalCount { get; set; }
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
