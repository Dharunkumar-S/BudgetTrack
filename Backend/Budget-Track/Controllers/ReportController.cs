// File: Controllers/ReportsController.cs
using Budget_Track.Models.DTOs.Reports;
using Budget_Track.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Budget_Track.Controllers
{
    [ApiController]
    [Route("api/reports")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _service;

        public ReportController(IReportService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get budget summary for a date range
        /// </summary>
        [HttpGet("period")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPeriodReport(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate
        )
        {
            try
            {
                var report = await _service.GetPeriodReportAsync(startDate, endDate);
                return Ok(report);
            }
            catch (Exception ex)
                when (ex.Message.Contains("Invalid") || ex.Message.Contains("must be"))
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { success = false, message = "Failed to generate period report" }
                );
            }
        }

        /// <summary>
        /// Get budget/expense statistics grouped by department
        /// </summary>
        [HttpGet("department")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetDepartmentReport()
        {
            try
            {
                var report = await _service.GetDepartmentReportAsync(null);
                return Ok(report);
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { success = false, message = "Failed to generate department report" }
                );
            }
        }

        /// <summary>
        /// Get detailed budget information with all expenses
        /// </summary>
        [HttpGet("budget")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetBudgetReport([FromQuery] string budgetCode)
        {
            try
            {
                var report = await _service.GetBudgetReportAsync(budgetCode);
                return Ok(report);
            }
            catch (Exception ex)
                when (ex.Message.Contains("not found") || ex.Message.Contains("does not exist"))
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { success = false, message = "Failed to generate budget report" }
                );
            }
        }
    }
}
