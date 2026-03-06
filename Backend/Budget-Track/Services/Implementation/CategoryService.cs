using Budget_Track.Models.DTOs.Category;
using Budget_Track.Repositories.Interfaces;
using Budget_Track.Services.Interfaces;

namespace Budget_Track.Services.Implementation
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _repository;

        public CategoryService(ICategoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<CategoryResponseDto>> GetAllCategoriesAsync()
        {
            return await _repository.GetAllCategoriesAsync();
        }

        public async Task<int> CreateCategoryAsync(CreateCategoryDto dto, int createdByUserID)
        {
            return await _repository.CreateCategoryAsync(dto, createdByUserID);
        }

        public async Task<bool> UpdateCategoryAsync(
            int categoryID,
            UpdateCategoryDto dto,
            int updatedByUserID
        )
        {
            return await _repository.UpdateCategoryAsync(categoryID, dto, updatedByUserID);
        }

        public async Task<bool> DeleteCategoryAsync(int categoryID, int deletedByUserID)
        {
            return await _repository.DeleteCategoryAsync(categoryID, deletedByUserID);
        }
    }
}
