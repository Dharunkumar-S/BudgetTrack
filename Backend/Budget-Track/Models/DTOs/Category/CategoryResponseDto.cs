#nullable enable
namespace Budget_Track.Models.DTOs.Category
{
    public class CategoryResponseDto
    {
        public int CategoryID { get; set; }
        public required string CategoryName { get; set; }
        public required string CategoryCode { get; set; }
        public bool IsActive { get; set; }
    }
}
