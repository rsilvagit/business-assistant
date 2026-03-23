namespace BusinessAssistant.Api.Services;

public interface ITokenCacheService
{
    Task StoreTokensAsync(string userId, string accessToken, string refreshToken, TimeSpan accessExpiration, TimeSpan refreshExpiration);
    Task<string?> GetAccessTokenAsync(string userId);
    Task<string?> GetUserIdByRefreshTokenAsync(string refreshToken);
    Task RevokeAllTokensAsync(string userId);
    Task<bool> IsTokenRevokedAsync(string token);
}
