using Budget_Track.Data;
using Budget_Track.Models.DTOs.Category;
using Budget_Track.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Budget_Track.Repositories.Implementation
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly BudgetTrackDbContext _context;

        public CategoryRepository(BudgetTrackDbContext context)
        {
            _context = context;
        }

        public async Task<List<CategoryResponseDto>> GetAllCategoriesAsync()
        {
            var categories = await _context
                .Database.SqlQueryRaw<CategoryResponseDto>(@"EXEC uspGetAllCategories")
                .ToListAsync();

            return categories;
        }

        public async Task<int> CreateCategoryAsync(CreateCategoryDto dto, int createdByUserID)
        {
            var categoryIDParam = new SqlParameter
            {
                ParameterName = "@NewCategoryID",
                SqlDbType = System.Data.SqlDbType.Int,
                Direction = System.Data.ParameterDirection.Output,
            };

            await _context.Database.ExecuteSqlRawAsync(
                @"EXEC uspCreateCategory 
                    @CategoryName, @CreatedByUserID, @NewCategoryID OUTPUT",
                new SqlParameter("@CategoryName", dto.CategoryName),
                new SqlParameter("@CreatedByUserID", createdByUserID),
                categoryIDParam
            );

            return (int)categoryIDParam.Value;
        }

        public async Task<bool> UpdateCategoryAsync(
            int categoryID,
            UpdateCategoryDto dto,
            int updatedByUserID
        )
        {
            await _context.Database.ExecuteSqlRawAsync(
                @"EXEC uspUpdateCategory 
                    @CategoryID, @CategoryName, @IsActive, @UpdatedByUserID",
                new SqlParameter("@CategoryID", categoryID),
                new SqlParameter("@CategoryName", dto.CategoryName),
                new SqlParameter("@IsActive", dto.IsActive),
                new SqlParameter("@UpdatedByUserID", updatedByUserID)
            );

            return true;
        }

        public async Task<bool> DeleteCategoryAsync(int categoryID, int deletedByUserID)
        {
            await _context.Database.ExecuteSqlRawAsync(
                @"EXEC uspDeleteCategory 
                    @CategoryID, @DeletedByUserID",
                new SqlParameter("@CategoryID", categoryID),
                new SqlParameter("@DeletedByUserID", deletedByUserID)
            );

            return true;
        }
    }
}
