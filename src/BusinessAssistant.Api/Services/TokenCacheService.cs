using Microsoft.Extensions.Caching.Distributed;

namespace BusinessAssistant.Api.Services;

public class TokenCacheService : ITokenCacheService
{
    private readonly IDistributedCache _cache;
    private const string TokenPrefix = "token:";
    private const string RevokedPrefix = "revoked:";

    public TokenCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task StoreTokenAsync(string userId, string token, TimeSpan expiration)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration
        };

        await _cache.SetStringAsync($"{TokenPrefix}{userId}", token, options);
    }

    public async Task<string?> GetTokenAsync(string userId)
    {
        return await _cache.GetStringAsync($"{TokenPrefix}{userId}");
    }

    public async Task RevokeTokenAsync(string userId)
    {
        var token = await GetTokenAsync(userId);
        if (token is null) return;

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2)
        };

        await _cache.SetStringAsync($"{RevokedPrefix}{token}", "revoked", options);
        await _cache.RemoveAsync($"{TokenPrefix}{userId}");
    }

    public async Task<bool> IsTokenRevokedAsync(string token)
    {
        var value = await _cache.GetStringAsync($"{RevokedPrefix}{token}");
        return value is not null;
    }
}
