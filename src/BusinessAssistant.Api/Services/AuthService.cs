using System.Security.Cryptography;
using BusinessAssistant.Api.Data;
using BusinessAssistant.Api.DTOs;
using BusinessAssistant.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BusinessAssistant.Api.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly ITokenCacheService _tokenCache;
    private readonly IConfiguration _configuration;

    public AuthService(
        AppDbContext context,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        ITokenCacheService tokenCache,
        IConfiguration configuration)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _tokenCache = tokenCache;
        _configuration = configuration;
    }

    public async Task<LoginResponse> RegisterAsync(RegisterRequest request)
    {
        var exists = await _context.Users.AnyAsync(u => u.Username == request.Username);
        if (exists)
            throw new InvalidOperationException("Username already exists.");

        var (hash, salt) = _passwordHasher.Hash(request.Password);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Role = "User"
        };

        var credential = new UserCredential
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            PasswordHash = hash,
            PasswordSalt = salt
        };

        _context.Users.Add(user);
        _context.UserCredentials.Add(credential);
        await _context.SaveChangesAsync();

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Credential)
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user?.Credential is null ||
            !_passwordHasher.Verify(request.Password, user.Credential.PasswordHash, user.Credential.PasswordSalt))
            throw new UnauthorizedAccessException("Invalid username or password.");

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<LoginResponse> RefreshTokenAsync(string refreshToken)
    {
        var userId = await _tokenCache.GetUserIdByRefreshTokenAsync(refreshToken);

        if (userId is null)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        var user = await _context.Users.FindAsync(Guid.Parse(userId));

        if (user is null)
            throw new UnauthorizedAccessException("User not found.");

        return await GenerateAuthResponseAsync(user);
    }

    public async Task LogoutAsync(string userId)
    {
        await _tokenCache.RevokeAllTokensAsync(userId);
    }

    private async Task<LoginResponse> GenerateAuthResponseAsync(User user)
    {
        var accessToken = _tokenService.GenerateToken(user);
        var expirationHours = double.Parse(_configuration["Jwt:ExpirationInHours"]!);
        var refreshTokenDays = int.Parse(_configuration["Jwt:RefreshTokenExpirationInDays"] ?? "7");
        var expiration = DateTime.UtcNow.AddHours(expirationHours);

        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        await _tokenCache.StoreTokensAsync(
            user.Id.ToString(),
            accessToken,
            refreshToken,
            TimeSpan.FromHours(expirationHours),
            TimeSpan.FromDays(refreshTokenDays));

        return new LoginResponse(accessToken, refreshToken, expiration);
    }
}
