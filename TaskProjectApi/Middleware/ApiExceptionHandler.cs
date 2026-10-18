using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TaskListProject.Application.Common;

namespace TaskReportApi.Middleware;

public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var rule = exception as BusinessRuleException;
        var status = rule is null ? StatusCodes.Status500InternalServerError : StatusCodes.Status400BadRequest;
        if (rule is null)
        {
            logger.LogError(exception, "Error processing {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
        }
        else
        {
            logger.LogWarning(exception, "Business rule rejected {Path}", httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = rule is null ? "An error occurred" : "Business rule validation failed",
            Detail = rule?.Message,
            Instance = httpContext.Request.Path,
            Extensions = { ["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier }
        }, options: null, contentType: "application/problem+json", cancellationToken: cancellationToken);
        return true;
    }
}
