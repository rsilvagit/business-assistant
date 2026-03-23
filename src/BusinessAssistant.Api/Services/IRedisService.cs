using StackExchange.Redis;

namespace BusinessAssistant.Api.Services;

public interface IRedisService
{
    Task SetAsync(string key, RedisValue value, TimeSpan? expiry = null);
    Task<string?> GetStringAsync(string key);
    Task<bool> DeleteAsync(string key);
}
