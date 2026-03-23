using System.Net;
using System.Text.Json;
using BusinessAssistant.Api.Exceptions;
using BusinessAssistant.Api.Middleware.Model;

namespace BusinessAssistant.Api.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, errors) = exception switch
        {
            BadRequest400Exception ex => (HttpStatusCode.BadRequest, BuildResponse(ex.Message, BadRequest400Exception.Errors)),
            Unauthorized401Exception ex => (HttpStatusCode.Unauthorized, BuildResponse(ex.Message, Unauthorized401Exception.Errors)),
            Forbidden403Exception ex => (HttpStatusCode.Forbidden, BuildResponse(ex.Message, Forbidden403Exception.Errors)),
            NotFound404Exception ex => (HttpStatusCode.NotFound, BuildResponse(ex.Message, NotFound404Exception.Errors)),
            Conflict409Exception ex => (HttpStatusCode.Conflict, BuildResponse(ex.Message, Conflict409Exception.Errors)),
            UnprocessableEntity422Exception ex => (HttpStatusCode.UnprocessableEntity, BuildResponse(ex.Message, UnprocessableEntity422Exception.Errors)),
            InternalServer500Exception ex => (HttpStatusCode.InternalServerError, BuildResponse(ex.Message, InternalServer500Exception.Errors)),
            _ => (HttpStatusCode.InternalServerError, BuildResponse("An unexpected error occurred.", []))
        };

        errors.StatusCode = statusCode;

        _logger.LogInformation("[ExceptionHandlingMiddleware] - Rota: {Path} - {Response}", context.Request.Path, errors);

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "[ExceptionHandlingMiddleware] - Rota: {Path}", context.Request.Path);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1));
        await context.Response.WriteAsync(JsonSerializer.Serialize(errors, options), cts.Token);
    }

    private static ErrorsResponse BuildResponse(string message, Dictionary<string, List<string>> errors)
    {
        var response = new ErrorsResponse { Messages = message };

        if (errors.Count > 0)
        {
            response.ValidationProperties = errors
                .Select(e => new ValidationProperty(e.Key, e.Value))
                .ToList();
        }

        return response;
    }
}
