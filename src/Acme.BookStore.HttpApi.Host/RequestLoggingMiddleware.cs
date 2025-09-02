using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        _logger.LogInformation("----- Incoming Request -----");
        _logger.LogInformation("Path: {Path} {Method}", context.Request.Path, context.Request.Method);

        foreach (var header in context.Request.Headers)
        {
            _logger.LogInformation("Header: {Key} = {Value}", header.Key, header.Value);
        }

        _logger.LogInformation("-----------------------------");

        await _next(context);
    }
}