
using Microsoft.AspNetCore.Mvc;

namespace HRIS.API.Controllers;

[ApiController]
[Route("api/error-lab")]
public class ErrorLabController : ControllerBase
{
    [HttpGet("boom")]
    public IActionResult Boom()
    {
        throw new InvalidOperationException(
            "Simulasi exception untuk pengujian global handler.");
    }
}
