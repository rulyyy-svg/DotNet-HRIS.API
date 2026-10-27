
using Microsoft.AspNetCore.Mvc;

namespace HRIS.API.Common.ExceptionHandlers;

public static class ProblemDetailsWriter
{
    public const string ErrorCodeKey = "errorCode";

    public static async ValueTask WriteAsync(
        IProblemDetailsService service,
        HttpContext httpContext,
        ProblemDetails problem,
        Exception exception,
        CancellationToken ct)
    {
        httpContext.Response.StatusCode =
            problem.Status ?? StatusCodes.Status500InternalServerError;

        var written = await service.TryWriteAsync(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problem,
                Exception = exception
            });

        // Fallback jika IProblemDetailsService tidak menulis respons.
        if (!written)
        {
            await httpContext.Response.WriteAsJsonAsync(problem, ct);
        }
    }
}
