using System.Net;
using System.Text.Json;
using Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Models;
using Models.Models;
using Serilog;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(RequestDelegate next, IHostEnvironment env)
    {
        _next = next;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        Log.Information("::::In Middleware::::");
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
        Log.Information("::::::End of Middleware::::");
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
{
    context.Response.ContentType = "application/json";

    var response = context.Response;
    var errorResponse = new ErrorDetails();

    switch (ex)
    {
        case HttpStatusCodeException httpEx:
            response.StatusCode = httpEx.StatusCode;
            errorResponse.Message = httpEx.Message;
            Log.Warning(ex, "Custom HttpStatusCodeException: {Message}", httpEx.Message);
            break;

        case ArgumentNullException:
        case ArgumentException:
            response.StatusCode = (int)HttpStatusCode.BadRequest;
            errorResponse.Message = ex.Message;
            Log.Warning(ex, "Bad Request: {Message}", ex.Message);
            break;

        case KeyNotFoundException:
            response.StatusCode = (int)HttpStatusCode.NotFound;
            errorResponse.Message = ex.Message;
            Log.Warning(ex, "Not Found: {Message}", ex.Message);
            break;

        case InvalidOperationException:
            response.StatusCode = (int)HttpStatusCode.Conflict;
            errorResponse.Message = ex.Message;
            Log.Warning(ex, "Invalid Operation: {Message}", ex.Message);
            break;

        case UnauthorizedAccessException:
            response.StatusCode = (int)HttpStatusCode.Unauthorized;
            errorResponse.Message = ex.Message;
            Log.Warning(ex, "Unauthorized Access: {Message}", ex.Message);
            break;

        case FormatException:
            response.StatusCode = (int)HttpStatusCode.BadRequest;
            errorResponse.Message = "Invalid format. " + ex.Message;
            Log.Warning(ex, "Format Error: {Message}", ex.Message);
            break;

        default:
            response.StatusCode = (int)HttpStatusCode.InternalServerError;
            errorResponse.Message = _env.IsDevelopment()
                ? ex.Message
                : "An unexpected error occurred.";
            errorResponse.StackTrace = _env.IsDevelopment() ? ex.StackTrace : null;
            Log.Error(ex, "Unhandled Exception: {Message}", ex.Message);
            break;
    }

    errorResponse.StatusCode = response.StatusCode;

    var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    var json = JsonSerializer.Serialize(errorResponse, options);

    await context.Response.WriteAsync(json);
}
}
