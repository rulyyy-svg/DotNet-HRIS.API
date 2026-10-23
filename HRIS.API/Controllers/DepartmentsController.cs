using HRIS.API.Data;
using HRIS.API.Entities;
using HRIS.API.DTOs.Departments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRIS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentsController(HrisDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DepartmentResponse>>> GetAll(
        CancellationToken ct)
    {
        var departments = await context.Departments
            .AsNoTracking()
            .Include(d => d.ParentDepartment)
            .OrderBy(d => d.Code)
            .Select(d => new DepartmentResponse(
                d.Id,
                d.Code,
                d.Name,
                d.ParentDepartmentId,
                d.ParentDepartment == null
                    ? null
                    : d.ParentDepartment.Name
            ))
            .ToListAsync(ct);

        return Ok(departments);
    }

    [HttpPost]
    public async Task<ActionResult<DepartmentResponse>> Create(
        [FromBody] CreateDepartmentRequest request,
        CancellationToken ct)
    {
        Department? parentDepartment = null;

        if (request.ParentDepartmentId.HasValue)
        {
            parentDepartment = await context.Departments
                .FirstOrDefaultAsync(
                    d => d.Id == request.ParentDepartmentId.Value,
                    ct);

            if (parentDepartment is null)
            {
                return BadRequest("Parent department tidak ditemukan.");
            }
        }

        var department = new Department
        {
            Id = Guid.CreateVersion7(),
            Code = request.Code,
            Name = request.Name,
            ParentDepartmentId = request.ParentDepartmentId
        };

        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine(ex.InnerException?.Message);
            throw;
        }

        var response = new DepartmentResponse(
            department.Id,
            department.Code,
            department.Name,
            department.ParentDepartmentId,
            parentDepartment?.Name
        );

        return StatusCode(StatusCodes.Status201Created, response);
    }
}