
using HRIS.API.Features.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRIS.API.Controllers;

public record LoginRequest(
    string Email,
    string Password);

[ApiController]
[Route("api/auth")]
public class AuthController(
    ISender sender,
    IUserCredentialService credentialService,
    ITokenService tokenService) : ControllerBase
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
    /// Validate credentials and issue a JWT access token.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<LoginResponse>(
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

        var (token, expiresAt) =
            tokenService.CreateToken(user);

        return Ok(
            new LoginResponse(token, expiresAt));
    }
}
