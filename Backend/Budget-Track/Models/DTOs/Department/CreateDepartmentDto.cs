#nullable enable
using System.ComponentModel.DataAnnotations;

namespace Budget_Track.Models.DTOs.Department
{
    public class CreateDepartmentDto
    {
        [Required]
        [MaxLength(100)]
        public required string DepartmentName { get; set; }
    }
}
