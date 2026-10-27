using HRIS.API.Common.Paging;
using HRIS.API.Entities;
using HRIS.API.Features.Departments.Queries.GetDepartmentTree;
using HRIS.API.Features.Employees.Commands.AssignManager;
using HRIS.API.Features.Employees.Commands.CreateEmployee;
using HRIS.API.Features.Employees.Commands.DeleteEmployee;
using HRIS.API.Features.Employees.Commands.TerminateEmployee;
using HRIS.API.Features.Employees.Commands.UpdateEmployee;
using HRIS.API.Features.Employees.Queries.GetEmployeeById;
using HRIS.API.Features.Employees.Queries.GetEmployees;
using HRIS.API.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HRIS.API.Controllers;

/// <summary>
/// Thin controller: translate HTTP to a command or query,
/// send it, then translate the result back to HTTP.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EmployeesController(ISender sender) : ControllerBase
{
    // GET: /api/employees
    [HttpGet]
    public async Task<ActionResult<PagedResult<EmployeeListDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] Guid? departmentId,
        [FromQuery] EmployeeStatus? status,
        [FromQuery] string? sortBy,
        [FromQuery] bool desc = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await sender.Send(
            new GetEmployeesQuery(
                search,
                departmentId,
                status,
                sortBy,
                desc,
                page,
                pageSize),
            ct);

        return Ok(result);
    }

    // GET: /api/employees/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EmployeeResponse>> GetById(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var employee = await sender.Send(
            new GetEmployeeByIdQuery(id), ct);

        return Ok(employee);
    }

    // POST: /api/employees
    [HttpPost]
    [ProducesResponseType<EmployeeResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<EmployeeResponse>> Create(
        [FromBody] CreateEmployeeRequest request,
        CancellationToken ct)
    {
        var id = await sender.Send(
            new CreateEmployeeCommand(request), ct);

        var employee = await sender.Send(
            new GetEmployeeByIdQuery(id), ct);

        return CreatedAtAction(
            nameof(GetById),
            new { id },
            employee);
    }

    // PUT: /api/employees/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateEmployeeRequest request,
        CancellationToken ct)
    {
        await sender.Send(
            new UpdateEmployeeCommand(id, request), ct);

        return NoContent();
    }

    // PUT: /api/employees/{id}/manager
    [HttpPut("{id:guid}/manager")]
    public async Task<IActionResult> AssignManager(
        [FromRoute] Guid id,
        [FromBody] AssignManagerRequest request,
        CancellationToken ct)
    {
        await sender.Send(
            new AssignManagerCommand(id, request.ManagerId), ct);

        return NoContent();
    }

    // POST: /api/employees/{id}/terminate
    [HttpPost("{id:guid}/terminate")]
    public async Task<IActionResult> Terminate(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        await sender.Send(
            new TerminateEmployeeCommand(id), ct);

        return NoContent();
    }

    // DELETE: /api/employees/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        await sender.Send(
            new DeleteEmployeeCommand(id), ct);

        return NoContent();
    }

    // GET: /api/employees/tree
    [HttpGet("tree")]
    public async Task<ActionResult<IReadOnlyCollection<DepartmentTreeDto>>> GetTree(
        CancellationToken ct)
    {
        var result = await sender.Send(
            new GetDepartmentTreeQuery(), ct);

        return Ok(result);
    }
}