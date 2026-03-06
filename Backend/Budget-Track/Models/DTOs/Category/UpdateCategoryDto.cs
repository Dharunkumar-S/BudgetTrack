#nullable enable
using System.ComponentModel.DataAnnotations;

namespace Budget_Track.Models.DTOs.Category
{
    public class UpdateCategoryDto
    {
        [Required]
        [MaxLength(100)]
        public required string CategoryName { get; set; }

        public bool IsActive { get; set; }
    }
}
