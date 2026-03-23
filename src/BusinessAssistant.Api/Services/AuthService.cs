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
            PasswordHash = hash,
            PasswordSalt = salt,
            Role = "User"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash, user.PasswordSalt))
            throw new UnauthorizedAccessException("Invalid username or password.");

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<LoginResponse> RefreshTokenAsync(string refreshToken)
    {
        var storedToken = await _context.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == refreshToken);

        if (storedToken is null || !storedToken.IsActive)
            throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        storedToken.RevokedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GenerateAuthResponseAsync(storedToken.User);
    }

    public async Task LogoutAsync(string userId)
    {
        var guidUserId = Guid.Parse(userId);

        var activeTokens = await _context.RefreshTokens
            .Where(r => r.UserId == guidUserId && r.RevokedAt == null)
            .ToListAsync();

        foreach (var token in activeTokens)
            token.RevokedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await _tokenCache.RevokeTokenAsync(userId);
    }

    private async Task<LoginResponse> GenerateAuthResponseAsync(User user)
    {
        var accessToken = _tokenService.GenerateToken(user);
        var expirationHours = double.Parse(_configuration["Jwt:ExpirationInHours"]!);
        var expiration = DateTime.UtcNow.AddHours(expirationHours);

        await _tokenCache.StoreTokenAsync(user.Id.ToString(), accessToken, TimeSpan.FromHours(expirationHours));

        var refreshTokenDays = int.Parse(_configuration["Jwt:RefreshTokenExpirationInDays"] ?? "7");
        var refreshToken = await CreateRefreshTokenAsync(user.Id, refreshTokenDays);

        return new LoginResponse(accessToken, refreshToken.Token, expiration);
    }

    private async Task<RefreshToken> CreateRefreshTokenAsync(Guid userId, int expirationDays)
    {
        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = token,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(expirationDays),
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return refreshToken;
    }
}
