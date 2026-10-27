using HRIS.API.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace HRIS.API.Common.ExceptionHandlers;

public class DomainExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<DomainExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken ct)
    {
        if (exception is not HrisException hrisException)
            return false;

        logger.LogWarning(
            "Request rejected with {StatusCode} {ErrorCode} ({ExceptionType}): {Reason}",
            hrisException.StatusCode,
            hrisException.ErrorCode,
            hrisException.GetType().Name,
            hrisException.Message);

        var problem = new ProblemDetails
        {
            Status = hrisException.StatusCode,
            Title = hrisException.Title,
            Detail = hrisException.Message
        };

        problem.Extensions[ProblemDetailsWriter.ErrorCodeKey] =
            hrisException.ErrorCode;

        await ProblemDetailsWriter.WriteAsync(
            problemDetailsService,
            httpContext,
            problem,
            exception,
            ct);

        return true;
    }
}