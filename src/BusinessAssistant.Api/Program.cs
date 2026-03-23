using BusinessAssistant.Api.Configurations;
using BusinessAssistant.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddDatabaseConfiguration(builder.Configuration)
    .AddRedisConfiguration(builder.Configuration)
    .AddJwtAuthentication(builder.Configuration)
    .AddRateLimitConfiguration()
    .AddSwaggerConfiguration()
    .AddApplicationServices();

var app = builder.Build();

app.UseSwaggerConfiguration();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.UseDatabaseMigration();

app.MapAuthEndpoints();
app.MapCustomerEndpoints();

app.Run();
