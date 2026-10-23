namespace HRIS.API.DTOs.Departments;

public record CreateDepartmentRequest(
    string Code,
    string Name,
    Guid? ParentDepartmentId);