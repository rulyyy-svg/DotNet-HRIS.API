using FluentValidation;
using HRIS.API.Data;
using Microsoft.EntityFrameworkCore;

namespace HRIS.API.Features.Departments.Commands.CreateDepartment;

public class CreateDepartmentCommandValidator
    : AbstractValidator<CreateDepartmentCommand>
{
    public CreateDepartmentCommandValidator(HrisDbContext db)
    {
        RuleFor(x => x.Request.Code)
            .NotEmpty()
            .MaximumLength(10);

        RuleFor(x => x.Request.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Request.ParentDepartmentId)
            .MustAsync(async (id, ct) =>
                await db.Departments.AnyAsync(
                    d => d.Id == id, ct))
            .WithMessage("Parent department does not exist.")
            .When(x => x.Request.ParentDepartmentId is not null);
    }
}