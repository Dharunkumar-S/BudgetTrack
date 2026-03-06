using Budget_Track.Models.DTOs.Category;
using Budget_Track.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Budget_Track.Controllers
{
    [Route("api/categories")]
    public class CategoryController : BaseApiController
    {
        private readonly ICategoryService _service;

        public CategoryController(ICategoryService service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Employee")]
        public async Task<IActionResult> GetAllCategories()
        {
            try
            {
                var categories = await _service.GetAllCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new { success = false, message = "Failed to retrieve categories" }
                );
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var categoryId = await _service.CreateCategoryAsync(dto, UserId);
                return CreatedAtAction(
                    nameof(GetAllCategories),
                    new { id = categoryId },
                    new { categoryId, message = "Category is created" }
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

        [HttpPut("{categoryID}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCategory(
            int categoryID,
            [FromBody] UpdateCategoryDto dto
        )
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _service.UpdateCategoryAsync(categoryID, dto, UserId);
                return Ok(new { success = result, message = "Category is updated" });
            }
            catch (Exception ex)
                when (ex.Message.Contains("Category not found")
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

        [HttpDelete("{categoryID}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCategory(int categoryID)
        {
            try
            {
                var result = await _service.DeleteCategoryAsync(categoryID, UserId);
                return Ok(new { success = result, message = "Category is deleted" });
            }
            catch (Exception ex)
                when (ex.Message.Contains("Category not found")
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