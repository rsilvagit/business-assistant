using BusinessAssistant.Api.Services;
using FluentValidation;

namespace BusinessAssistant.Api.Configurations;

public static class DependencyInjectionConfiguration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICustomerService, CustomerService>();

        services.AddValidatorsFromAssemblyContaining<Program>();

        return services;
    }
}
