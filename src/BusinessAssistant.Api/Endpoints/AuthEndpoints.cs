using BusinessAssistant.Api.DTOs;
using BusinessAssistant.Api.Exceptions;
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
                throw BadRequest400Exception.ValidationResult(validation.Errors);

            await authService.SignupAsync(request);
            return Results.Created();
        })
        .AllowAnonymous()
        .Produces(StatusCodes.Status201Created)
        .WithName("Signup")
        .WithOpenApi();

        group.MapPost("/login", async (
            LoginDto request,
            IValidator<LoginDto> validator,
            IAuthService authService) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
                throw BadRequest400Exception.ValidationResult(validation.Errors);

            var response = await authService.LoginAsync(request);
            return Results.Ok(response);
        })
        .AllowAnonymous()
        .Produces<AuthResponse>()
        .WithName("Login")
        .WithOpenApi();

        group.MapPost("/refresh-token", async (
            RequestRefreshTokenDto request,
            IValidator<RequestRefreshTokenDto> validator,
            IAuthService authService) =>
        {
            var validation = await validator.ValidateAsync(request);
            if (!validation.IsValid)
                throw BadRequest400Exception.ValidationResult(validation.Errors);

            var response = await authService.RefreshTokenAsync(request.RefreshToken);
            return Results.Ok(response);
        })
        .AllowAnonymous()
        .Produces<AuthResponse>()
        .WithName("RefreshToken")
        .WithOpenApi();

        group.MapPost("/logout", async (
            HttpContext httpContext,
            IUserClaims userClaims,
            IAuthService authService) =>
        {
            if (userClaims.AccountId == Guid.Empty)
                throw new Unauthorized401Exception("User not authenticated.");

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
