using System.Diagnostics;
using System.Threading.Tasks;

namespace UserManagement.Web.Middleware;

public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var request = context.Request;

        logger.LogInformation("HTTP {Method} {Path} started", request.Method, request.Path);

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();
            var response = context.Response;

            logger.LogInformation("HTTP {Method} {Path} completed with {StatusCode} in {ElapsedMs}ms",
                request.Method,
                request.Path,
                response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
    }
}
