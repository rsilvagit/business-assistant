namespace BusinessAssistant.Api.Services;

public interface ITokenCacheService
{
    Task StoreTokenAsync(string userId, string token, TimeSpan expiration);
    Task<string?> GetTokenAsync(string userId);
    Task RevokeTokenAsync(string userId);
    Task<bool> IsTokenRevokedAsync(string token);
}
