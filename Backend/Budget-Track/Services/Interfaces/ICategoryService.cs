using Budget_Track.Models.DTOs.Category;

namespace Budget_Track.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryResponseDto>> GetAllCategoriesAsync();
        Task<int> CreateCategoryAsync(CreateCategoryDto dto, int createdByUserID);
        Task<bool> UpdateCategoryAsync(int categoryID, UpdateCategoryDto dto, int updatedByUserID);
        Task<bool> DeleteCategoryAsync(int categoryID, int deletedByUserID);
    }
}
