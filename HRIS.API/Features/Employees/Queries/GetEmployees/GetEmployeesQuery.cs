
using HRIS.API.Common.Paging;
using HRIS.API.Data;
using HRIS.API.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRIS.API.Features.Employees.Queries.GetEmployees;

public record GetEmployeesQuery(
    string? Search,
    Guid? DepartmentId,
    EmployeeStatus? Status,
    string? SortBy,
    bool Desc = false,
    int Page = 1,
    int PageSize = 10
) : IRequest<PagedResult<EmployeeListDto>>;

public record EmployeeListDto(
    Guid Id,
    string EmployeeNumber,
    string FullName,
    string DepartmentName,
    string PositionTitle,
    string? ManagerName,
    EmployeeStatus Status
);

public class GetEmployeesQueryHandler(HrisDbContext db)
    : IRequestHandler<
        GetEmployeesQuery,
        PagedResult<EmployeeListDto>>
{
    public async Task<PagedResult<EmployeeListDto>> Handle(
        GetEmployeesQuery query,
        CancellationToken ct)
    {
        IQueryable<Employee> employees =
            db.Employees.AsNoTracking();

        // 1. Search berdasarkan nama atau nomor Employee
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();

            employees = employees.Where(e =>
                e.FullName.Contains(search) ||
                e.EmployeeNumber.Contains(search));
        }

        // 2. Filter berdasarkan Department
        if (query.DepartmentId.HasValue)
        {
            employees = employees.Where(e =>
                e.DepartmentId == query.DepartmentId.Value);
        }

        // 3. Default hanya Employee aktif
        var status = query.Status ?? EmployeeStatus.Active;

        employees = employees.Where(e =>
            e.EmployeeStatus == status);

        // 4. Sorting dengan pilihan kolom yang diizinkan
        employees = query.SortBy?.ToLowerInvariant() switch
        {
            "hiredate" => query.Desc
                ? employees
                    .OrderByDescending(e => e.HireDate)
                    .ThenBy(e => e.EmployeeNumber)
                : employees
                    .OrderBy(e => e.HireDate)
                    .ThenBy(e => e.EmployeeNumber),

            "number" => query.Desc
                ? employees
                    .OrderByDescending(e => e.EmployeeNumber)
                : employees
                    .OrderBy(e => e.EmployeeNumber),

            _ => query.Desc
                ? employees
                    .OrderByDescending(e => e.FullName)
                    .ThenBy(e => e.EmployeeNumber)
                : employees
                    .OrderBy(e => e.FullName)
                    .ThenBy(e => e.EmployeeNumber)
        };

        // 5. Projection ke DTO, lalu paging
        return await employees
            .Select(e => new EmployeeListDto(
                e.Id,
                e.EmployeeNumber,
                e.FullName,
                e.Department.Name,
                e.Position.Title,
                e.Manager != null
                    ? e.Manager.FullName
                    : null,
                e.EmployeeStatus))
            .ToPagedResultAsync(
                query.Page,
                query.PageSize,
                ct: ct);
    }
}
