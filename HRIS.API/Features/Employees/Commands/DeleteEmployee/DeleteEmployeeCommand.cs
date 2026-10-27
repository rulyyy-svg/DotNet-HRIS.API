using HRIS.API.Common.Exceptions;
using HRIS.API.Data.Repositories;
using MediatR;

namespace HRIS.API.Features.Employees.Commands.DeleteEmployee;

public record DeleteEmployeeCommand(Guid Id) : IRequest;

public class DeleteEmployeeCommandHandler(IUnitOfWork uow)
    : IRequestHandler<DeleteEmployeeCommand>
{
    public async Task Handle(
        DeleteEmployeeCommand command,
        CancellationToken ct)
    {
        var employee = await uow.Employees.GetByIdAsync(
            command.Id, ct)
            ?? throw new NotFoundException(
                "Employee", command.Id);

        var directReports =
            await uow.Employees.GetDirectReportsAsync(
                employee.Id, ct);

        if (directReports.Count > 0)
        {
            throw new ConflictException(
                $"Employee '{employee.Id}' still manages other employees. Reassign them first.");
        }

        uow.Employees.Delete(employee);

        await uow.SaveChangesAsync(ct);
    }
}