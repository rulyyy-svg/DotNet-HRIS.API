
using HRIS.API.Entities;

namespace HRIS.API.Models;

public static class EmployeeMappings
{
    public static Employee ToEntity(
        this CreateEmployeeRequest request,
        DateTimeOffset createdAt,
        EmployeeStatus employeeStatus) =>
        new()
        {
            Id = Guid.CreateVersion7(),
            EmployeeNumber = request.EmployeeNumber,
            FullName = request.FullName,
            Email = request.Email,
            HireDate = request.HireDate,
            CreatedAt = createdAt,
            EmployeeStatus = employeeStatus,
            TerminatedAt = null,
            DepartmentId = request.DepartmentId,
            PositionId = request.PositionId,
            ManagerId = request.ManagerId
        };

    public static Employee ToEntity(
        this Employee employee,
        UpdateEmployeeRequest request)
    {
        employee.FullName = request.FullName;
        employee.DepartmentId = request.DepartmentId;
        employee.PositionId = request.PositionId;
        employee.ManagerId = request.ManagerId;

        return employee;
    }

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
