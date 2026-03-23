using BusinessAssistant.Api.DTOs;

namespace BusinessAssistant.Api.Services;

public interface IAuthService
{
    Task<LoginResponse> RegisterAsync(RegisterRequest request);
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<LoginResponse> RefreshTokenAsync(string refreshToken);
    Task LogoutAsync(string userId);
}
