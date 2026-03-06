#nullable enable
using Microsoft.EntityFrameworkCore;

namespace Budget_Track.Models.DTOs.Reports
{
    public class ExpenseCountDto
    {
        public int TotalExpenseCount { get; set; }
        public int PendingExpenseCount { get; set; }
        public int ApprovedExpenseCount { get; set; }
        public int RejectedExpenseCount { get; set; }
    }
}
