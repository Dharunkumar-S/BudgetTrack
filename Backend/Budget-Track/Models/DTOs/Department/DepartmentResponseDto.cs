#nullable enable
namespace Budget_Track.Models.DTOs.Department
{
    public class DepartmentResponseDto
    {
        public int DepartmentID { get; set; }
        public required string DepartmentName { get; set; }
        public required string DepartmentCode { get; set; }
        public bool IsActive { get; set; }
    }
}
