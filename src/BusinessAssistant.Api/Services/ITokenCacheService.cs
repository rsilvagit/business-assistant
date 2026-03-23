namespace BusinessAssistant.Api.Services;

public interface ITokenCacheService
{
    Task StoreRefreshTokenAsync(string refreshToken, string accessToken, TimeSpan expiry);
    Task<string?> GetAccessTokenByRefreshTokenAsync(string refreshToken);
    Task BlacklistTokenAsync(Guid accountId, string token);
    Task<bool> IsTokenBlacklistedAsync(Guid accountId, string token);
    Task DeleteRefreshTokenAsync(string refreshToken);
}
