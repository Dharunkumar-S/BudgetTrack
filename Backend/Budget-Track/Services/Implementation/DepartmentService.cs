using Budget_Track.Models.DTOs.Department;
using Budget_Track.Repositories.Interfaces;
using Budget_Track.Services.Interfaces;

namespace Budget_Track.Services.Implementation
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IDepartmentRepository _repository;

        public DepartmentService(IDepartmentRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<DepartmentResponseDto>> GetAllDepartmentsAsync()
        {
            return await _repository.GetAllDepartmentsAsync();
        }

        public async Task<int> CreateDepartmentAsync(CreateDepartmentDto dto, int createdByUserID)
        {
            return await _repository.CreateDepartmentAsync(dto, createdByUserID);
        }

        public async Task<bool> UpdateDepartmentAsync(
            int departmentID,
            UpdateDepartmentDto dto,
            int updatedByUserID
        )
        {
            return await _repository.UpdateDepartmentAsync(departmentID, dto, updatedByUserID);
        }

        public async Task<bool> DeleteDepartmentAsync(int departmentID, int deletedByUserID)
        {
            return await _repository.DeleteDepartmentAsync(departmentID, deletedByUserID);
        }
    }
}
