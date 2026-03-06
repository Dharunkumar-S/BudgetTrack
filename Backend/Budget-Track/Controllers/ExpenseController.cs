using Budget_Track.Models.DTOs.Expense;
using Budget_Track.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Budget_Track.Controllers
{
    [Route("api/expenses")]
    public class ExpenseController : BaseApiController
    {
        private readonly IExpenseService _service;

        public ExpenseController(IExpenseService service)
        {
            _service = service;
        }

        [HttpGet("stats")]
        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<IActionResult> GetExpenseStatistics(
            [FromQuery] int? budgetID,
            [FromQuery] string? title = null,
            [FromQuery] string? budgetTitle = null,
            [FromQuery] string? status = null,
            [FromQuery] string? categoryName = null,
            [FromQuery] string? submittedUserName = null,
            [FromQuery] string? submittedByEmployeeID = null,
            [FromQuery] string? departmentName = null,
            [FromQuery] bool myExpensesOnly = false
        )
        {
            var role =
                User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
                ?? User.FindFirst("role")?.Value;
            var managerIdStr = User.FindFirst("ManagerId")?.Value;
            int? managerId = string.IsNullOrEmpty(managerIdStr)
                ? (int?)null
                : int.Parse(managerIdStr);

            var filters = new ManagedExpenseFilterDto
            {
                BudgetID = budgetID,
                Title = title,
                BudgetTitle = budgetTitle,
                Status = status,
                CategoryName = categoryName,
                SubmittedUserName = submittedUserName,
                SubmittedByEmployeeID = submittedByEmployeeID,
                DepartmentName = departmentName,
                MyExpensesOnly = myExpensesOnly,
                CurrentUserId = UserId,
                Role = role,
                ManagerId = managerId,
            };

            var stats = await _service.GetExpenseStatisticsAsync(filters);
            return Ok(stats);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<IActionResult> GetAllExpenses(
            [FromQuery] string? title = null,
            [FromQuery] string? budgetTitle = null,
            [FromQuery] string? status = null,
            [FromQuery] string? categoryName = null,
            [FromQuery] string? submittedUserName = null,
            [FromQuery] string? submittedByEmployeeID = null,
            [FromQuery] string? departmentName = null,
            [FromQuery] string? sortBy = "SubmittedDate",
            [FromQuery] string? sortOrder = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10
        )
        {
            try
            {
                var filters = new ExpenseFilterDto
                {
                    Title = title,
                    BudgetTitle = budgetTitle,
                    Status = status,
                    CategoryName = categoryName,
                    SubmittedUserName = submittedUserName,
                    SubmittedByEmployeeID = submittedByEmployeeID,
                    DepartmentName = departmentName,
                    SortBy = sortBy,
                    SortOrder = sortOrder,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                };

                var result = await _service.GetAllExpensesAsync(filters);
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

        [HttpGet("managed")]
        [Authorize(Roles = "Manager,Employee")]
        public async Task<IActionResult> GetManagedExpenses(
            [FromQuery] string? title = null,
            [FromQuery] string? status = null,
            [FromQuery] string? categoryName = null,
            [FromQuery] string? submittedUserName = null,
            [FromQuery] string? submittedByEmployeeID = null,
            [FromQuery] bool myExpensesOnly = false,
            [FromQuery] string? sortBy = "SubmittedDate",
            [FromQuery] string? sortOrder = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10
        )
        {
            try
            {
                var role =
                    User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
                    ?? User.FindFirst("role")?.Value;
                var managerIdValue = User.FindFirst("ManagerId")?.Value;
                int? managerId = string.IsNullOrEmpty(managerIdValue)
                    ? null
                    : int.Parse(managerIdValue);

                var filters = new ManagedExpenseFilterDto
                {
                    Title = title,
                    Status = status,
                    CategoryName = categoryName,
                    SubmittedUserName = submittedUserName,
                    SubmittedByEmployeeID = submittedByEmployeeID,
                    MyExpensesOnly = myExpensesOnly,
                    SortBy = sortBy,
                    SortOrder = sortOrder,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    CurrentUserId = UserId,
                    Role = role,
                    ManagerId = managerId,
                };

                var result = await _service.GetManagedExpensesAsync(filters);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { success = false, message = "Failed to retrieve managed expenses" }
                );
            }
        }

        [HttpPost]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> CreateExpense([FromBody] CreateExpenseDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var expenseId = await _service.CreateExpenseAsync(dto, UserId);
                return CreatedAtAction(
                    nameof(GetAllExpenses),
                    new { },
                    new { expenseId, message = "Expense is created" }
                );
            }
            catch (Exception ex)
                when (ex.Message.Contains("not found") || ex.Message.Contains("does not exist"))
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
                when (ex.Message.Contains("insufficient") || ex.Message.Contains("exceeds"))
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        private static bool ContainsMessage(Exception ex, string keyword) =>
            (ex.Message?.Contains(keyword, StringComparison.OrdinalIgnoreCase) == true)
            || (
                ex.InnerException?.Message?.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                == true
            );

        private static string GetMessage(Exception ex) => ex.InnerException?.Message ?? ex.Message;

        [HttpPut("status/{expenseID}")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> UpdateExpenseStatus(
            int expenseID,
            [FromBody] UpdateExpenseStatusDto dto
        )
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                string? approvalComments =
                    dto.Status == Models.Enums.ExpenseStatus.Approved ? dto.Comments : null;
                string? rejectionReason =
                    dto.Status == Models.Enums.ExpenseStatus.Rejected ? dto.Reason : null;

                var result = await _service.UpdateExpenseStatusAsync(
                    expenseID,
                    (int)dto.Status,
                    UserId,
                    approvalComments,
                    rejectionReason
                );

                var statusMessage =
                    dto.Status == Models.Enums.ExpenseStatus.Approved
                        ? "Expense is approved"
                        : "Expense is rejected";

                return Ok(new { success = result, message = statusMessage });
            }
            catch (Exception ex)
                when (ContainsMessage(ex, "Expense not found")
                    || ContainsMessage(ex, "does not exist")
                )
            {
                return NotFound(new { success = false, message = GetMessage(ex) });
            }
            catch (Exception ex) when (ContainsMessage(ex, "No changes detected"))
            {
                return BadRequest(new { success = false, message = "No changes made" });
            }
            catch (Exception ex)
                when (ContainsMessage(ex, "already")
                    || ContainsMessage(ex, "cannot")
                    || ContainsMessage(ex, "Invalid status")
                    || ContainsMessage(ex, "cannot be updated")
                    || ContainsMessage(ex, "Only pending")
                )
            {
                return BadRequest(new { success = false, message = GetMessage(ex) });
            }
        }
    }
}
