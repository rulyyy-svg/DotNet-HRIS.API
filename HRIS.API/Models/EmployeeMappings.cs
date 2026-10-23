using HRIS.API.Entities;

namespace HRIS.API.Models;

public static class EmployeeMappingExtensions
{
    public static EmployeeResponse ToResponse(this Employee employee) =>
        new(
            employee.Id,
            employee.EmployeeNumber,
            employee.FullName,
            employee.Email,
            employee.HireDate,
            employee.EmployeeStatus,
            employee.Department.Name,
            employee.Position.Title,
            employee.Manager?.FullName,
            employee.CreatedAt
        );
}