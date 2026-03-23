using Microsoft.Extensions.Caching.Distributed;

namespace BusinessAssistant.Api.Services;

public class TokenCacheService : ITokenCacheService
{
    private readonly IDistributedCache _cache;
    private const string AccessTokenPrefix = "access:";
    private const string RefreshTokenPrefix = "refresh:";
    private const string RefreshLookupPrefix = "refresh_lookup:";
    private const string RevokedPrefix = "revoked:";

    public TokenCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task StoreTokensAsync(string userId, string accessToken, string refreshToken, TimeSpan accessExpiration, TimeSpan refreshExpiration)
    {
        var oldRefreshToken = await _cache.GetStringAsync($"{RefreshTokenPrefix}{userId}");
        if (oldRefreshToken is not null)
            await _cache.RemoveAsync($"{RefreshLookupPrefix}{oldRefreshToken}");

        var accessOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = accessExpiration
        };

        var refreshOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = refreshExpiration
        };

        await _cache.SetStringAsync($"{AccessTokenPrefix}{userId}", accessToken, accessOptions);
        await _cache.SetStringAsync($"{RefreshTokenPrefix}{userId}", refreshToken, refreshOptions);
        await _cache.SetStringAsync($"{RefreshLookupPrefix}{refreshToken}", userId, refreshOptions);
    }

    public async Task<string?> GetAccessTokenAsync(string userId)
    {
        return await _cache.GetStringAsync($"{AccessTokenPrefix}{userId}");
    }

    public async Task<string?> GetUserIdByRefreshTokenAsync(string refreshToken)
    {
        return await _cache.GetStringAsync($"{RefreshLookupPrefix}{refreshToken}");
    }

    public async Task RevokeAllTokensAsync(string userId)
    {
        var accessToken = await _cache.GetStringAsync($"{AccessTokenPrefix}{userId}");
        var refreshToken = await _cache.GetStringAsync($"{RefreshTokenPrefix}{userId}");

        if (accessToken is not null)
        {
            var revokedOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2)
            };
            await _cache.SetStringAsync($"{RevokedPrefix}{accessToken}", "revoked", revokedOptions);
        }

        if (refreshToken is not null)
            await _cache.RemoveAsync($"{RefreshLookupPrefix}{refreshToken}");

        await _cache.RemoveAsync($"{AccessTokenPrefix}{userId}");
        await _cache.RemoveAsync($"{RefreshTokenPrefix}{userId}");
    }

    public async Task<bool> IsTokenRevokedAsync(string token)
    {
        var value = await _cache.GetStringAsync($"{RevokedPrefix}{token}");
        return value is not null;
    }
}
