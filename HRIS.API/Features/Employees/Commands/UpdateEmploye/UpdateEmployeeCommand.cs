using HRIS.API.Common.Exceptions;
using HRIS.API.Data;
using HRIS.API.Data.Repositories;
using HRIS.API.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRIS.API.Features.Employees.Commands.UpdateEmployee;

public record UpdateEmployeeCommand(
    Guid Id,
    UpdateEmployeeRequest Request
) : IRequest<bool>;

public class UpdateEmployeeCommandHandler(
    IUnitOfWork uow)
    : IRequestHandler<UpdateEmployeeCommand, bool>
{
    public async Task<bool> Handle(
        UpdateEmployeeCommand command,
        CancellationToken ct)
    {
        var employee = await uow.Employees.GetByIdAsync(
            command.Id, ct);

        if (employee is null)
        {
            throw new NotFoundException(
                $"Employee dengan ID '{command.Id}' tidak ditemukan.");
        }

        employee.ToEntity(command.Request);

        uow.Employees.Update(employee);

        try
        {
            await uow.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
        {
            throw new DuplicateEmployeeException(
                "Employee number atau email sudah digunakan.",
                ex);
        }

        return true;
    }
}