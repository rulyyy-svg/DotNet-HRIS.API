
using HRIS.API.DTOs.Departments;
using HRIS.API.Entities;

namespace HRIS.API.Models;

public static class DepartmentMappings
{
    public static Department ToEntity(
        this CreateDepartmentRequest request)
    {
        return new Department
        {
            Id = Guid.CreateVersion7(),
            Code = request.Code,
            Name = request.Name,
            ParentDepartmentId = request.ParentDepartmentId
        };
    }
}
