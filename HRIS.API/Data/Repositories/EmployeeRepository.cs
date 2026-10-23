
using HRIS.API.Entities;
using HRIS.API.Models;
using Microsoft.EntityFrameworkCore;

namespace HRIS.API.Data.Repositories;

public class EmployeeRepository(HrisDbContext context)
    : Repository<Employee>(context), IEmployeeRepository
{
    private IQueryable<EmployeeResponse> Responses()
        => Context.Employees
            .AsNoTracking()
            .Select(e => e.ToResponse());

    public async Task<IReadOnlyCollection<EmployeeResponse>> GetResponseAsync(
        CancellationToken ct = default)
        => await Responses()
            .OrderBy(e => e.FullName)
            .ToListAsync(ct);

    public async Task<EmployeeResponse?> GetResponseAsync(
        Guid id,
        CancellationToken ct = default)
        => await Responses()
            .Where(e => e.Id == id)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyCollection<Employee>> GetDirectReportsAsync(
        Guid managerId,
        CancellationToken ct = default)
        => await Context.Employees
            .Where(e => e.ManagerId == managerId)
            .ToListAsync(ct);
}
