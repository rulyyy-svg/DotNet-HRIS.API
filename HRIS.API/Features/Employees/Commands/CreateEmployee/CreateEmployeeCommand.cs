using HRIS.API.Common.Exceptions;
using HRIS.API.Data;
using HRIS.API.Data.Repositories;
using HRIS.API.Entities;
using HRIS.API.Extensions;
using HRIS.API.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRIS.API.Features.Employees.Commands.CreateEmployee;

public record CreateEmployeeCommand(
    CreateEmployeeRequest Request) : IRequest<Guid>;

public class CreateEmployeeCommandHandler(
    IUnitOfWork uow,
    TimeProvider timeProvider,
    ILogger<CreateEmployeeCommandHandler> logger)
    : IRequestHandler<CreateEmployeeCommand, Guid>
{
    public async Task<Guid> Handle(
        CreateEmployeeCommand command,
        CancellationToken ct)
    {
        var request = command.Request with
        {
            Email = command.Request.Email.NormalizeEmail()
        };

        if (await uow.Employees.EmployeeNumberExistsAsync(
                request.EmployeeNumber, ct))
        {
            throw new DuplicateEmployeeException(
                $"Employee number '{request.EmployeeNumber}' is already used.");
        }

        if (await uow.Employees.EmailExistsAsync(request.Email, ct))
        {
            throw new DuplicateEmployeeException(
                "The email is already used by another employee.");
        }

        var employee = request.ToEntity(
            timeProvider.GetUtcNow(),
            EmployeeStatus.Active);

        await uow.Employees.AddAsync(employee, ct);

        try
        {
            await uow.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
            when (ex.IsUniqueConstraintViolation())
        {
            throw new DuplicateEmployeeException(
                "Employee number or email is already used.", ex);
        }

        logger.LogInformation(
            "Employee {EmployeeId} created",
            employee.Id);

        return employee.Id;
    }
}