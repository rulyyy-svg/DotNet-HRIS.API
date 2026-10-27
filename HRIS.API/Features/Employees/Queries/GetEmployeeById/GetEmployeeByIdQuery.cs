using HRIS.API.Common.Exceptions;
using HRIS.API.Data;
using HRIS.API.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRIS.API.Features.Employees.Queries.GetEmployeeById;

public record GetEmployeeByIdQuery(Guid Id)
    : IRequest<EmployeeResponse>;

public class GetEmployeeByIdQueryHandler(HrisDbContext db)
    : IRequestHandler<GetEmployeeByIdQuery, EmployeeResponse>
{
    public async Task<EmployeeResponse> Handle(
        GetEmployeeByIdQuery query,
        CancellationToken ct)
    {
        var employee = await db.Employees
            .AsNoTracking()
            .Include(e => e.Department)
            .Include(e => e.Position)
            .Include(e => e.Manager)
            .FirstOrDefaultAsync(e => e.Id == query.Id, ct)
            ?? throw new NotFoundException(
                "Employee", query.Id);

        return employee.ToResponse();
    }
}