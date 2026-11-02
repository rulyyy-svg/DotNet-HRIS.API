
using System.Security.Claims;
using HRIS.API.Common.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRIS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            UserId = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value,

            Email = User.FindFirst(
                ClaimTypes.Email)?.Value,

            Roles = User.FindAll(
                ClaimTypes.Role)
                .Select(c => c.Value),

            EmployeeId = User.FindFirst(
                HrisClaimTypes.EmployeeId)?.Value
        });
    }
}
