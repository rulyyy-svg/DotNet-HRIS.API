namespace HRIS.API.DTOs.Departments;

public record DepartmentResponse(
    Guid Id,
    string Code,
    string Name,
    Guid? ParentDepartmentId,
    string? ParentDepartmentName);