using System.Diagnostics;

namespace IceFactoryManagmentSystem.Middleware;

/// <summary>
/// Middleware that adds a unique request ID to each request for tracking and logging.
/// </summary>
public class RequestIdMiddleware
{
    private readonly RequestDelegate _next;

    public RequestIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestId = context.TraceIdentifier;
        context.Items["RequestId"] = requestId;
        context.Response.Headers.Add("X-Request-ID", requestId);

        await _next(context);
    }
}