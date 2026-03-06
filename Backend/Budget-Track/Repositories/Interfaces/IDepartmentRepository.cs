using Budget_Track.Models.DTOs.Department;

namespace Budget_Track.Repositories.Interfaces
{
    public interface IDepartmentRepository
    {
        Task<List<DepartmentResponseDto>> GetAllDepartmentsAsync();
        Task<int> CreateDepartmentAsync(CreateDepartmentDto dto, int createdByUserID);
        Task<bool> UpdateDepartmentAsync(int departmentID, UpdateDepartmentDto dto, int updatedByUserID);
        Task<bool> DeleteDepartmentAsync(int departmentID, int deletedByUserID);
    }
}
