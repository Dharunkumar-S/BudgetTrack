#nullable enable
namespace Budget_Track.Models.DTOs.Budget
{
    public class BudgetFilterDto
    {
        public string? Title { get; set; }
        public string? Code { get; set; }
        public List<string>? Status { get; set; }
        public bool? IsDeleted { get; set; }

        /// <summary>
        /// Sort field: "Title", "Code", or "CreatedDate" (default: "CreatedDate")
        /// </summary>
        public string? SortBy { get; set; } = "CreatedDate";

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
    }
}
