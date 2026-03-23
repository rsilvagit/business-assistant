using System.Security.Claims;
using BusinessAssistant.Api.DTOs;
using BusinessAssistant.Api.Services;
using FluentValidation;

namespace BusinessAssistant.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Auth");

        group.MapPost("/register", async (
            RegisterRequest request,
            IValidator<RegisterRequest> validator,
            IAuthService authService) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            try
            {
                var response = await authService.RegisterAsync(request);
                return Results.Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
        })
        .AllowAnonymous()
        .Produces<LoginResponse>()
        .ProducesValidationProblem()
        .WithName("Register")
        .WithOpenApi();

        group.MapPost("/login", async (
            LoginRequest request,
            IValidator<LoginRequest> validator,
            IAuthService authService) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            try
            {
                var response = await authService.LoginAsync(request);
                return Results.Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
        })
        .AllowAnonymous()
        .Produces<LoginResponse>()
        .ProducesValidationProblem()
        .WithName("Login")
        .WithOpenApi();

        group.MapPost("/refresh", async (
            RefreshTokenRequest request,
            IAuthService authService) =>
        {
            try
            {
                var response = await authService.RefreshTokenAsync(request.RefreshToken);
                return Results.Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
        })
        .AllowAnonymous()
        .Produces<LoginResponse>()
        .WithName("RefreshToken")
        .WithOpenApi();

        group.MapPost("/logout", async (HttpContext httpContext, IAuthService authService) =>
        {
            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
                return Results.Unauthorized();

            await authService.LogoutAsync(userId);
            return Results.Ok(new { message = "Logged out successfully." });
        })
        .RequireAuthorization()
        .Produces(StatusCodes.Status200OK)
        .WithName("Logout")
        .WithOpenApi();
    }
}
