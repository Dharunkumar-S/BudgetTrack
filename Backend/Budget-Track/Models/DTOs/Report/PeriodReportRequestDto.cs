#nullable enable
using System.ComponentModel.DataAnnotations;

namespace Budget_Track.Models.DTOs.Reports
{
    /// <summary>
    /// Request DTO for Period Report
    /// </summary>
    public class PeriodReportRequestDto
    {
        [Required(ErrorMessage = "Start date is required")]
        public required DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required")]
        public required DateTime EndDate { get; set; }
    }
}
