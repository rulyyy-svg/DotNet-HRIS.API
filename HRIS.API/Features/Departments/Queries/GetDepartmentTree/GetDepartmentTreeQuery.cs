
using HRIS.API.Data;
using HRIS.API.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRIS.API.Features.Departments.Queries.GetDepartmentTree;

public record SubDepartmentDto(
    Guid Id,
    string Code,
    string Name,
    int Headcount);

public record DepartmentTreeDto(
    Guid Id,
    string Code,
    string Name,
    int Headcount,
    List<SubDepartmentDto> SubDepartments);

public record GetDepartmentTreeQuery
    : IRequest<IReadOnlyCollection<DepartmentTreeDto>>;

public class GetDepartmentTreeQueryHandler(HrisDbContext db)
    : IRequestHandler<GetDepartmentTreeQuery,
        IReadOnlyCollection<DepartmentTreeDto>>
{
    public async Task<IReadOnlyCollection<DepartmentTreeDto>> Handle(
        GetDepartmentTreeQuery query,
        CancellationToken ct)
    {
        var departments = await db.Departments
            .AsNoTracking()
            .Where(d => d.ParentDepartmentId == null)
            .OrderBy(d => d.Name)
            .Select(d => new
            {
                d.Id,
                d.Code,
                d.Name,
                Headcount = d.Employees.Count(e =>
                    e.EmployeeStatus == EmployeeStatus.Active)
            })
            .ToListAsync(ct);

        var departmentIds = departments
            .Select(d => d.Id)
            .ToList();

        var subDepartments = await db.Departments
            .AsNoTracking()
            .Where(d => d.ParentDepartmentId.HasValue &&
                        departmentIds.Contains(d.ParentDepartmentId.Value))
            .Select(d => new
            {
                d.Id,
                d.Code,
                d.Name,
                ParentDepartmentId = d.ParentDepartmentId!.Value,
                Headcount = d.Employees.Count(e =>
                    e.EmployeeStatus == EmployeeStatus.Active)
            })
            .OrderBy(d => d.Name)
            .ToListAsync(ct);

        return departments
            .Select(d => new DepartmentTreeDto(
                d.Id,
                d.Code,
                d.Name,
                d.Headcount,
                subDepartments
                    .Where(s => s.ParentDepartmentId == d.Id)
                    .Select(s => new SubDepartmentDto(
                        s.Id,
                        s.Code,
                        s.Name,
                        s.Headcount))
                    .ToList()))
            .ToList();
    }
}
