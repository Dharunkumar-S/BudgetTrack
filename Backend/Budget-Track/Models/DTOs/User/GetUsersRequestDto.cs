#nullable enable
using Budget_Track.Models.Enums;

namespace Budget_Track.Models.DTOs.User
{
    public class GetUsersRequestDto
    {
        public int? RoleId { get; set; }
        public string? EmployeeId { get; set; }
        public bool? IsDeleted { get; set; }
        public bool? IsActive { get; set; }
        public string SortBy { get; set; } = "CreatedDate";
        public string SortOrder { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
