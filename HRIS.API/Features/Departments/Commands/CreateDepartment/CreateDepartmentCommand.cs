using HRIS.API.DTOs.Departments;
using HRIS.API.Models;
using MediatR;

namespace HRIS.API.Features.Departments.Commands.CreateDepartment;

public record CreateDepartmentCommand(
    CreateDepartmentRequest Request
) : IRequest<Guid>;