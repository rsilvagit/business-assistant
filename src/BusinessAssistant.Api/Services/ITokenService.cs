using BusinessAssistant.Api.Models;

namespace BusinessAssistant.Api.Services;

public interface ITokenService
{
    string GenerateToken(User user);
    string GenerateRefreshToken();
}
