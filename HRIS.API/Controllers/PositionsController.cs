using HRIS.API.Data;
using HRIS.API.Entities;
using HRIS.API.DTOs.Positions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRIS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PositionsController(HrisDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PositionResponse>>> GetAll(
        CancellationToken ct)
    {
        var positions = await context.Positions
            .AsNoTracking()
            .OrderBy(p => p.Level)
            .ThenBy(p => p.Title)
            .Select(p => new PositionResponse(
                p.Id,
                p.Title,
                p.Level
            ))
            .ToListAsync(ct);

        return Ok(positions);
    }

    [HttpPost]
    public async Task<ActionResult<PositionResponse>> Create(
        [FromBody] CreatePositionRequest request,
        CancellationToken ct)
    {
        var position = new Position
        {
            Id = Guid.CreateVersion7(),
            Title = request.Title,
            Level = request.Level
        };

        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine(ex.ToString());
            throw;
        }

        var response = new PositionResponse(
            position.Id,
            position.Title,
            position.Level
        );

        return StatusCode(StatusCodes.Status201Created, response);
    }
}