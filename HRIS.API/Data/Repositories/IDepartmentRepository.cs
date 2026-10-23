using HRIS.API.DTOs.Departments;
using HRIS.API.Entities;
using HRIS.API.Models;

namespace HRIS.API.Data.Repositories;

public interface IDepartmentRepository : IRepository<Department>
{
    Task<IReadOnlyCollection<DepartmentResponse>> GetResponseAsync(
        CancellationToken ct = default);
}