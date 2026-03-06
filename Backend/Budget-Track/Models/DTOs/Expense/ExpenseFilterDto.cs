#nullable enable
namespace Budget_Track.Models.DTOs.Expense
{
    public class ExpenseFilterDto
    {
        public int? BudgetID { get; set; }
        public string? Status { get; set; }
        public string? CategoryName { get; set; }
        public string? SubmittedUserName { get; set; }
        public string? SubmittedByEmployeeID { get; set; }
        public int? SubmittedByUserID { get; set; }

        /// <summary>
        /// Filter by expense title (uses LIKE '%value%')
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Filter by budget title (uses LIKE '%value%')
        /// </summary>
        public string? BudgetTitle { get; set; }

        /// <summary>
        /// Filter by department name (uses LIKE '%value%')
        /// </summary>
        public string? DepartmentName { get; set; }

        /// <summary>
        /// Sort field: "SubmittedDate", "Amount", "Title" (default: "SubmittedDate")
        /// </summary>
        public string? SortBy { get; set; } = "SubmittedDate";

        /// <summary>
        /// Sort order: "asc" or "desc" (default: "desc")
        /// </summary>
        public string? SortOrder { get; set; } = "desc";

        /// <summary>
        /// Page number for pagination (default: 1)
        /// </summary>
        public int? PageNumber { get; set; }

        /// <summary>
        /// Page size for pagination (default: 10, max: 100)
        /// </summary>
        public int? PageSize { get; set; }

        public int? CurrentUserId { get; set; }
        public string? Role { get; set; }
        public int? ManagerId { get; set; }
    }
}
