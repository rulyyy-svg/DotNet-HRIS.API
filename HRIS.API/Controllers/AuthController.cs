
using HRIS.API.Features.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace HRIS.API.Controllers;

public record LoginRequest(
    string Email,
    string Password);

public record LoginResult(
    Guid UserId,
    Guid? EmployeeId,
    string Email,
    IReadOnlyCollection<string> Roles);

[ApiController]
[Route("api/auth")]
public class AuthController(
    ISender sender,
    IUserCredentialService credentialService)
    : ControllerBase
{
    /// <summary>
    /// Activate an account for an existing employee.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType<RegisterResponse>(
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status409Conflict)]
    [ProducesResponseType(
        StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Register(
        RegisterCommand command,
        CancellationToken ct)
    {
        var result = await sender.Send(command, ct);

        return StatusCode(
            StatusCodes.Status201Created,
            result);
    }

    /// <summary>
    /// Validate email and password.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<LoginResult>(
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        LoginRequest request,
        CancellationToken ct)
    {
        var user = await credentialService.ValidateAsync(
            request.Email,
            request.Password,
            ct);

        if (user is null)
        {
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Invalid email or password.");
        }

        return Ok(
            new LoginResult(
                user.Id,
                user.EmployeeId,
                user.Email,
                user.Roles));
    }
}
