using HRIS.API.DTOs.Departments;
using HRIS.API.Entities;
using HRIS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRIS.API.Data.Repositories;

public class DepartmentRepository(HrisDbContext context)
    : Repository<Department>(context), IDepartmentRepository
{
    public async Task<IReadOnlyCollection<DepartmentResponse>> GetResponseAsync(
        CancellationToken ct = default)
    {
        return await Context.Departments
            .AsNoTracking()
            .OrderBy(d => d.Code)
            .Select(d => new DepartmentResponse(
                d.Id,
                d.Code,
                d.Name,
                d.ParentDepartmentId,
                d.ParentDepartment == null
                    ? null
                    : d.ParentDepartment.Name))
            .ToListAsync(ct);
    }
}