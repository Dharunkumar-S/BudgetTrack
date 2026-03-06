using Budget_Track.Data;
using Budget_Track.Models.DTOs.Department;
using Budget_Track.Models.Entities;
using Budget_Track.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Budget_Track.Repositories.Implementation
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly BudgetTrackDbContext _context;

        public DepartmentRepository(BudgetTrackDbContext context)
        {
            _context = context;
        }

        public async Task<List<DepartmentResponseDto>> GetAllDepartmentsAsync()
        {
            var departments = await _context
                .Database.SqlQueryRaw<DepartmentResponseDto>(@"EXEC uspGetAllDepartments")
                .ToListAsync();

            return departments;
        }

        public async Task<int> CreateDepartmentAsync(CreateDepartmentDto dto, int createdByUserID)
        {
            var departmentIDParam = new SqlParameter
            {
                ParameterName = "@NewDepartmentID",
                SqlDbType = System.Data.SqlDbType.Int,
                Direction = System.Data.ParameterDirection.Output,
            };

            await _context.Database.ExecuteSqlRawAsync(
                @"EXEC uspCreateDepartment 
                    @DepartmentName, @CreatedByUserID, @NewDepartmentID OUTPUT",
                new SqlParameter("@DepartmentName", dto.DepartmentName),
                new SqlParameter("@CreatedByUserID", createdByUserID),
                departmentIDParam
            );

            return (int)departmentIDParam.Value;
        }

        public async Task<bool> UpdateDepartmentAsync(
            int departmentID,
            UpdateDepartmentDto dto,
            int updatedByUserID
        )
        {
            await _context.Database.ExecuteSqlRawAsync(
                @"EXEC uspUpdateDepartment 
                    @DepartmentID, @DepartmentName, @IsActive, @UpdatedByUserID",
                new SqlParameter("@DepartmentID", departmentID),
                new SqlParameter("@DepartmentName", dto.DepartmentName),
                new SqlParameter("@IsActive", dto.IsActive),
                new SqlParameter("@UpdatedByUserID", updatedByUserID)
            );

            return true;
        }

        public async Task<bool> DeleteDepartmentAsync(int departmentID, int deletedByUserID)
        {
            await _context.Database.ExecuteSqlRawAsync(
                @"EXEC uspDeleteDepartment 
                    @DepartmentID, @DeletedByUserID",
                new SqlParameter("@DepartmentID", departmentID),
                new SqlParameter("@DeletedByUserID", deletedByUserID)
            );

            return true;
        }
    }
}
