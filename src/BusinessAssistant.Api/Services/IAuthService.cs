using BusinessAssistant.Api.DTOs;

namespace BusinessAssistant.Api.Services;

public interface IAuthService
{
    Task<AuthResponse> SignupAsync(SignupDto request);
    Task<AuthResponse> LoginAsync(LoginDto request);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
    Task LogoutAsync(Guid accountId, string token);
}
