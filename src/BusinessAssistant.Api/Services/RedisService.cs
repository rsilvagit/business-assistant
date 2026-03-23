using StackExchange.Redis;

namespace BusinessAssistant.Api.Services;

public class RedisService : IRedisService
{
    private readonly IDatabase _database;

    public RedisService(IConnectionMultiplexer connectionMultiplexer)
    {
        _database = connectionMultiplexer.GetDatabase();
    }

    public async Task SetAsync(string key, RedisValue value, TimeSpan? expiry = null)
    {
        if (expiry.HasValue)
            await _database.StringSetAsync(key, value, new Expiration(expiry.Value));
        else
            await _database.StringSetAsync(key, value);
    }

    public async Task<string?> GetStringAsync(string key)
    {
        var value = await _database.StringGetAsync(key);
        return value.HasValue ? value.ToString() : null;
    }

    public async Task<bool> DeleteAsync(string key)
    {
        return await _database.KeyDeleteAsync(key);
    }
}
