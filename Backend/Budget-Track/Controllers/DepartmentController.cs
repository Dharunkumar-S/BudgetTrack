using Budget_Track.Models.DTOs.Department;
using Budget_Track.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Budget_Track.Controllers
{
    [Route("api/departments")]
    public class DepartmentController : BaseApiController
    {
        private readonly IDepartmentService _service;

        public DepartmentController(IDepartmentService service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<IActionResult> GetAllDepartments()
        {
            try
            {
                var departments = await _service.GetAllDepartmentsAsync();
                return Ok(departments);
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { success = false, message = "Failed to retrieve departments" }
                );
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateDepartment([FromBody] CreateDepartmentDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var departmentId = await _service.CreateDepartmentAsync(dto, UserId);
                return CreatedAtAction(
                    nameof(GetAllDepartments),
                    new { id = departmentId },
                    new { departmentId, message = "Department is created" }
                );
            }
            catch (Exception ex)
                when (ex.Message.Contains("already exists") || ex.Message.Contains("duplicate"))
            {
                return Conflict(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { success = false, message = ex.Message }
                );
            }
        }

        [HttpPut("{departmentID}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateDepartment(
            int departmentID,
            [FromBody] UpdateDepartmentDto dto
        )
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _service.UpdateDepartmentAsync(departmentID, dto, UserId);
                return Ok(new { success = result, message = "Department is updated" });
            }
            catch (Exception ex)
                when (ex.Message.Contains("Department not found")
                    || ex.Message.Contains("does not exist")
                )
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { success = false, message = ex.Message }
                );
            }
        }

        [HttpDelete("{departmentID}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteDepartment(int departmentID)
        {
            try
            {
                var result = await _service.DeleteDepartmentAsync(departmentID, UserId);
                return Ok(new { success = result, message = "Department is deleted" });
            }
            catch (Exception ex)
                when (ex.Message.Contains("Department not found")
                    || ex.Message.Contains("does not exist")
                )
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                // Return the specific message (e.g., from RAISERROR in SQL)
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { success = false, message = ex.Message }
                );
            }
        }
    }
}