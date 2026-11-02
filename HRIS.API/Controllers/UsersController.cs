using HRIS.API.Common.Security;
using HRIS.API.Features.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRIS.API.Controllers;

public record AssignRoleRequest(string Role);

[ApiController]
[Route("api/users")]
[Authorize(Roles = Roles.Admin)]
public class UsersController(ISender sender) : ControllerBase
{
    [HttpPost("{userId:guid}/roles")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AssignRole(
        Guid userId,
        [FromBody] AssignRoleRequest body,
        CancellationToken ct)
    {
        await sender.Send(
            new AssignRoleCommand(userId, body.Role),
            ct);

        return NoContent();
    }
}