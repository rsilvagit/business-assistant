namespace BusinessAssistant.Api.Services;

public class TokenCacheService : ITokenCacheService
{
    private readonly IRedisService _redis;
    private static readonly TimeSpan BlacklistTtl = TimeSpan.FromDays(7);

    public TokenCacheService(IRedisService redis)
    {
        _redis = redis;
    }

    public async Task StoreRefreshTokenAsync(string refreshToken, string accessToken, TimeSpan expiry)
    {
        await _redis.SetAsync($"token:refresh:{refreshToken}", accessToken, expiry);
    }

    public async Task<string?> GetAccessTokenByRefreshTokenAsync(string refreshToken)
    {
        return await _redis.GetStringAsync($"token:refresh:{refreshToken}");
    }

    public async Task BlacklistTokenAsync(Guid accountId, string token)
    {
        var tokenPrefix = token.Length >= 15 ? token[..15] : token;
        await _redis.SetAsync($"token:blacklist:{accountId}:{tokenPrefix}", "true", BlacklistTtl);
    }

    public async Task<bool> IsTokenBlacklistedAsync(Guid accountId, string token)
    {
        var tokenPrefix = token.Length >= 15 ? token[..15] : token;
        var value = await _redis.GetStringAsync($"token:blacklist:{accountId}:{tokenPrefix}");
        return value is not null;
    }

    public async Task DeleteRefreshTokenAsync(string refreshToken)
    {
        await _redis.DeleteAsync($"token:refresh:{refreshToken}");
    }
}
