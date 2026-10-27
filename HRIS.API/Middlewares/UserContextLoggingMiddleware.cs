
using Serilog.Context;
using System.Security.Claims;

namespace HRIS.API.Middlewares;

public class UserContextLoggingMiddleware(
    RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var userId = context.User
            .FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? "Anonymous";

        using (LogContext.PushProperty("UserId", userId))
        {
            await next(context);
        }
    }
}
