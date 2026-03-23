using BusinessAssistant.Api.DTOs;
using BusinessAssistant.Api.Services;
using FluentValidation;

namespace BusinessAssistant.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth")
            .WithTags("Auth");

        group.MapPost("/signup", async (
            SignupDto request,
            IValidator<SignupDto> validator,
            IAuthService authService) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            try
            {
                await authService.SignupAsync(request);
                return Results.Created();
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
        })
        .AllowAnonymous()
        .Produces(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .WithName("Signup")
        .WithOpenApi();

        group.MapPost("/login", async (
            LoginDto request,
            IValidator<LoginDto> validator,
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
        .Produces<AuthResponse>()
        .ProducesValidationProblem()
        .WithName("Login")
        .WithOpenApi();

        group.MapPost("/refresh-token", async (
            RequestRefreshTokenDto request,
            IValidator<RequestRefreshTokenDto> validator,
            IAuthService authService) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

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
        .Produces<AuthResponse>()
        .ProducesValidationProblem()
        .WithName("RefreshToken")
        .WithOpenApi();

        group.MapPost("/logout", async (
            HttpContext httpContext,
            IUserClaims userClaims,
            IAuthService authService) =>
        {
            if (userClaims.AccountId == Guid.Empty)
                return Results.Unauthorized();

            var token = httpContext.Request.Headers.Authorization
                .ToString().Replace("Bearer ", "");

            await authService.LogoutAsync(userClaims.AccountId, token);
            return Results.Ok(new { message = "Logged out successfully." });
        })
        .RequireAuthorization()
        .Produces(StatusCodes.Status200OK)
        .WithName("Logout")
        .WithOpenApi();
    }
}
