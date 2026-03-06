using Budget_Track.Models.DTOs.Budget;
using Budget_Track.Models.DTOs.Expense;
using Budget_Track.Models.Enums;
using Budget_Track.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Budget_Track.Controllers
{
    [Route("api/budgets")]
    public class BudgetController : BaseApiController
    {
        private readonly IBudgetService _budgetService;
        private readonly ILogger<BudgetController> _logger;

        public BudgetController(IBudgetService budgetService, ILogger<BudgetController> logger)
        {
            _budgetService = budgetService;
            _logger = logger;
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllBudgets(
            [FromQuery] string? title,
            [FromQuery] string? code,
            [FromQuery] List<int>? status,
            [FromQuery] bool? isDeleted,
            [FromQuery] string sortBy = "CreatedDate",
            [FromQuery] string sortOrder = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10
        )
        {
            try
            {
                var filters = new BudgetFilterDto
                {
                    Title = title,
                    Code = code,
                    Status = status?.Select(s => s.ToString()).ToList(),
                    IsDeleted = isDeleted,
                    SortBy = sortBy,
                    SortOrder = sortOrder,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                };

                var result = await _budgetService.GetAllBudgetsAsync(filters);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { success = false, message = "Failed to retrieve budgets" }
                );
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<IActionResult> GetBudgetsByCreatedByUserIdWithPagination(
            [FromQuery] string? title,
            [FromQuery] string? code,
            [FromQuery] List<int>? status,
            [FromQuery] bool? isDeleted,
            [FromQuery] string sortBy = "CreatedDate",
            [FromQuery] string sortOrder = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10
        )
        {
            try
            {
                var roleClaim =
                    User.FindFirst(System.Security.Claims.ClaimTypes.Role)
                    ?? User.FindFirst("role");

                if (roleClaim == null)
                {
                    return Unauthorized(new { success = false, message = "Role not found" });
                }

                int createdByUserId;

                if (roleClaim.Value.Equals("Employee", StringComparison.OrdinalIgnoreCase))
                {
                    var managerIdClaim = User.FindFirst("ManagerId");
                    if (managerIdClaim == null)
                    {
                        return BadRequest(new { success = false, message = "Manager ID missing" });
                    }

                    createdByUserId = int.Parse(managerIdClaim.Value);
                }
                else
                {
                    createdByUserId = UserId;
                }

                var filters = new BudgetFilterDto
                {
                    Title = title,
                    Code = code,
                    Status = status?.Select(s => s.ToString()).ToList(),
                    IsDeleted = isDeleted,
                    SortBy = sortBy,
                    SortOrder = sortOrder,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                };

                var result = await _budgetService.GetBudgetsByCreatedByUserIdWithPaginationAsync(
                    createdByUserId,
                    filters
                );
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { success = false, message = "Failed to retrieve budgets" }
                );
            }
        }

        [HttpPost]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> CreateBudget([FromBody] CreateBudgetDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var budgetID = await _budgetService.CreateBudgetAsync(dto, UserId);
                return Created(
                    nameof(GetAllBudgets),
                    new { success = true, message = "Budget is created" }
                );
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex) when (IsDuplicateBudgetKey(ex))
            {
                return Conflict(new { success = false, message = GetDuplicateBudgetMessage(ex) });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating budget: {ex.Message}");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { success = false, message = "Failed to create budget" }
                );
            }
        }

        [HttpPut("{budgetID}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> UpdateBudget(int budgetID, [FromBody] UpdateBudgetDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _budgetService.UpdateBudgetAsync(budgetID, dto, UserId);
                return Ok(new { success = true, message = "Budget is updated" });
            }
            catch (Exception ex) when (IsDuplicateBudgetKey(ex))
            {
                return Conflict(new { success = false, message = GetDuplicateBudgetMessage(ex) });
            }
            catch (Exception ex)
                when (ex.Message.Contains("No changes detected")
                    || ex.InnerException?.Message.Contains("No changes detected") == true
                )
            {
                return BadRequest(new { success = false, message = "No changes made" });
            }
            catch (Exception ex)
                when (ex.Message.Contains("Budget not found")
                    || ex.Message.Contains("has already been deleted")
                )
            {
                return NotFound(new { success = false, message = "Budget not found" });
            }
            catch (Exception ex) when (ex.Message.Contains("can only update budgets you created"))
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating budget {budgetID}: {ex.Message}\nInnerException: {ex.InnerException?.Message}\nStackTrace: {ex.StackTrace}");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { success = false, message = "Failed to update budget", error = ex.Message }
                );
            }
        }

        [HttpDelete("{budgetID}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> DeleteBudget(int budgetID)
        {
            try
            {
                await _budgetService.DeleteBudgetAsync(budgetID, UserId);
                return Ok(new { success = true, message = "Budget is deleted" });
            }
            catch (Exception ex)
                when (ex.Message.Contains("Budget not found")
                    || ex.Message.Contains("has already been deleted")
                )
            {
                return NotFound(new { success = false, message = "Budget not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting budget {budgetID}: {ex.Message}");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { success = false, message = "Failed to delete budget" }
                );
            }
        }

        [HttpGet("{budgetID}/expenses")]
        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<IActionResult> GetExpensesByBudgetID(
            int budgetID,
            [FromServices] IExpenseService expenseService,
            [FromQuery] string? title = null,
            [FromQuery] string? status = null,
            [FromQuery] string? categoryName = null,
            [FromQuery] string? submittedUserName = null,
            [FromQuery] string? submittedByEmployeeID = null,
            [FromQuery] string? sortBy = "SubmittedDate",
            [FromQuery] string sortOrder = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10
        )
        {
            try
            {
                // Verify budget exists
                var budget = await _budgetService.GetBudgetByIdAsync(budgetID);
                if (budget == null)
                    return NotFound(new { success = false, message = "Budget not found" });

                // Enforce same ownership rules as GET /api/budgets
                var roleClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

                if (!string.Equals(roleClaim, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    int authorisedCreatorId;

                    if (string.Equals(roleClaim, "Employee", StringComparison.OrdinalIgnoreCase))
                    {
                        // Employee may only view budgets created by their own manager
                        var managerIdClaim = User.FindFirst("ManagerId")?.Value;
                        if (managerIdClaim == null)
                            return Forbid();
                        authorisedCreatorId = int.Parse(managerIdClaim);
                    }
                    else
                    {
                        // Manager may only view budgets they created
                        authorisedCreatorId = UserId;
                    }

                    if (budget.CreatedByUserID != authorisedCreatorId)
                        return Forbid();
                }

                var filters = new ExpenseFilterDto
                {
                    Title = title,
                    Status = status,
                    CategoryName = categoryName,
                    SubmittedUserName = submittedUserName,
                    SubmittedByEmployeeID = submittedByEmployeeID,
                    SortBy = sortBy,
                    SortOrder = sortOrder,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                };

                var result = await expenseService.GetExpensesByBudgetIDAsync(budgetID, filters);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { success = false, message = "Failed to retrieve expenses" }
                );
            }
        }

        private static bool IsDuplicateBudgetKey(Exception ex)
        {
            var msg = ex.Message + (ex.InnerException?.Message ?? string.Empty);
            return msg.Contains("duplicate key", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("IX_tBudget_Title", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("IX_tBudget_Code", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("already exists", StringComparison.OrdinalIgnoreCase);
        }

        private static string GetDuplicateBudgetMessage(Exception ex)
        {
            var msg = ex.InnerException?.Message ?? ex.Message;
            if (msg.Contains("IX_tBudget_Title", StringComparison.OrdinalIgnoreCase))
                return "Title already in use";
            if (msg.Contains("IX_tBudget_Code", StringComparison.OrdinalIgnoreCase))
                return "Code already in use";
            return "Title or code already exists";
        }
    }
}
