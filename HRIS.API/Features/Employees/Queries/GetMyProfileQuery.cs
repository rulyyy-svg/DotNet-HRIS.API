using HRIS.API.Common.Exceptions;
using HRIS.API.Common.Security;
using HRIS.API.Data;
using HRIS.API.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRIS.API.Features.Employees.Queries.GetMyProfile;

public record GetMyProfileQuery
    : IRequest<EmployeeResponse>;

public class GetMyProfileQueryHandler(
    HrisDbContext db,
    ICurrentUser currentUser)
    : IRequestHandler<GetMyProfileQuery, EmployeeResponse>
{
    public async Task<EmployeeResponse> Handle(
        GetMyProfileQuery query,
        CancellationToken ct)
    {
        var employeeId = currentUser.EmployeeId
            ?? throw new NotFoundException(
                "Employee",
                "current user");

        var employee = await db.Employees
            .AsNoTracking()
            .Include(e => e.Department)
            .Include(e => e.Position)
            .Include(e => e.Manager)
            .FirstOrDefaultAsync(
                e => e.Id == employeeId,
                ct)
            ?? throw new NotFoundException(
                "Employee",
                employeeId);

        return employee.ToResponse();
    }
}