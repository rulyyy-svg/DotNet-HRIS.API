using HRIS.API.Common.Exceptions;
using HRIS.API.Data.Repositories;
using HRIS.API.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HRIS.API.Features.Employees.Commands.TerminateEmployee;

public record TerminateEmployeeCommand(Guid EmployeeId) : IRequest;

public class TerminateEmployeeCommandHandler(
    IUnitOfWork uow,
    TimeProvider timeProvider,
    ILogger<TerminateEmployeeCommandHandler> logger)
    : IRequestHandler<TerminateEmployeeCommand>
{
    public async Task Handle(
        TerminateEmployeeCommand command,
        CancellationToken ct)
    {
        var employee = await uow.Employees.GetByIdAsync(
            command.EmployeeId, ct)
            ?? throw new NotFoundException(
                "Employee", command.EmployeeId);

        if (employee.EmployeeStatus == EmployeeStatus.Terminated)
        {
            throw new EmployeeAlreadyTerminatedException(employee.Id);
        }

        employee.EmployeeStatus = EmployeeStatus.Terminated;

        employee.TerminatedAt = DateOnly.FromDateTime(
            timeProvider.GetUtcNow().UtcDateTime);

        var directReports =
            await uow.Employees.GetDirectReportsAsync(employee.Id, ct);

        foreach (var report in directReports)
        {
            report.ManagerId = employee.ManagerId;
        }

        await uow.SaveChangesAsync(ct);

        logger.LogInformation(
            "Employee {EmployeeId} terminated; {ReassignedCount} direct reports moved to manager {ManagerId}",
            employee.Id,
            directReports.Count,
            employee.ManagerId);
    }
}