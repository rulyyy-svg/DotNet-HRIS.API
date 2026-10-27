
using HRIS.API.Data;
using HRIS.API.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRIS.API.Features.Reports.Queries.GetHeadcountByDepartment;

public record HeadcountByDepartmentDto(
    string DepartmentCode,
    string DepartmentName,
    int Headcount
);

public record GetHeadcountByDepartmentQuery
    : IRequest<IReadOnlyCollection<HeadcountByDepartmentDto>>;

public class GetHeadcountByDepartmentQueryHandler(
    HrisDbContext db)
    : IRequestHandler<
        GetHeadcountByDepartmentQuery,
        IReadOnlyCollection<HeadcountByDepartmentDto>>
{
    public async Task<IReadOnlyCollection<HeadcountByDepartmentDto>>
        Handle(
            GetHeadcountByDepartmentQuery query,
            CancellationToken ct)
    {
        return await db.Employees
            .AsNoTracking()
            .Where(e =>
                e.EmployeeStatus == EmployeeStatus.Active)
            .GroupBy(e => new
            {
                e.Department.Code,
                e.Department.Name
            })
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key.Code)
            .Select(g => new HeadcountByDepartmentDto(
                g.Key.Code,
                g.Key.Name,
                g.Count()))
            .ToListAsync(ct);
    }
}
