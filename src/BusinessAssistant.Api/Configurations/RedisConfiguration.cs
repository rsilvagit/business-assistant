using StackExchange.Redis;

namespace BusinessAssistant.Api.Configurations;

public static class RedisConfiguration
{
    public static IServiceCollection AddRedisConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString("Redis")!;
        var options = ConfigurationOptions.Parse(redisConnectionString);
        options.AbortOnConnectFail = false;

        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(options));

        return services;
    }
}
