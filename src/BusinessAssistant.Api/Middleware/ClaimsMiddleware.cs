using System.Security.Claims;
using BusinessAssistant.Api.Services;

namespace BusinessAssistant.Api.Middleware;

public class ClaimsMiddleware
{
    private readonly RequestDelegate _next;

    public ClaimsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IUserClaims userClaims)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var accountId = context.User.FindFirstValue("accountId");
            if (Guid.TryParse(accountId, out var parsedId))
                userClaims.AccountId = parsedId;

            userClaims.Name = context.User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
            userClaims.Email = context.User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
            userClaims.Role = context.User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        }

        await _next(context);
    }
}
