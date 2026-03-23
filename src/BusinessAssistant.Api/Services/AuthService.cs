using System.IdentityModel.Tokens.Jwt;
using BusinessAssistant.Api.Data;
using BusinessAssistant.Api.DTOs;
using BusinessAssistant.Api.Exceptions;
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

    public async Task<AuthResponse> SignupAsync(SignupDto request)
    {
        var exists = await _context.Users.AnyAsync(u => u.Email == request.Email);
        if (exists)
            throw new Conflict409Exception("Email already registered.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = request.Email.Split('@')[0],
            Email = request.Email,
            Role = "User",
            Status = AccountStatus.Active
        };

        var passwordModel = new PasswordModel
        {
            Id = Guid.NewGuid(),
            Salt = Guid.NewGuid(),
            AccountId = user.Id,
            Actived = true
        };

        var saltObject = new SaltObject(passwordModel.Id, passwordModel.Salt);
        passwordModel.Password = _passwordHasher.Hash(request.Password, saltObject);

        _context.Users.Add(user);
        _context.Passwords.Add(passwordModel);
        await _context.SaveChangesAsync();

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginDto request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user is null)
            throw Forbidden403Exception.EmailOrPassword();

        var password = await _context.Passwords
            .FirstOrDefaultAsync(p => p.AccountId == user.Id && p.Actived == true);

        if (password is null)
            throw Forbidden403Exception.EmailOrPassword();

        var saltObject = new SaltObject(password.Id, password.Salt);
        if (!_passwordHasher.Verify(request.Password, password.Password, saltObject))
            throw Forbidden403Exception.EmailOrPassword();

        if (user.Status == AccountStatus.Pending)
        {
            user.Status = AccountStatus.Active;
            await _context.SaveChangesAsync();
        }

        return await GenerateAuthResponseAsync(user);
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        var storedAccessToken = await _tokenCache.GetAccessTokenByRefreshTokenAsync(refreshToken);
        if (storedAccessToken is null)
            throw Forbidden403Exception.RefreshToken();

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(storedAccessToken);
        var accountIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "accountId")?.Value;

        if (accountIdClaim is null || !Guid.TryParse(accountIdClaim, out var accountId))
            throw Forbidden403Exception.TokenInvalid();

        var user = await _context.Users.FindAsync(accountId)
            ?? throw new NotFound404Exception("User not found.");

        await _tokenCache.BlacklistTokenAsync(accountId, storedAccessToken);
        await _tokenCache.DeleteRefreshTokenAsync(refreshToken);

        return await GenerateAuthResponseAsync(user);
    }

    public async Task LogoutAsync(Guid accountId, string token)
    {
        await _tokenCache.BlacklistTokenAsync(accountId, token);
    }

    private async Task<AuthResponse> GenerateAuthResponseAsync(User user)
    {
        var accessToken = _tokenService.GenerateToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        var refreshTokenHours = int.Parse(_configuration["Jwt:RefreshTokenExpirationInHours"] ?? "24");
        await _tokenCache.StoreRefreshTokenAsync(refreshToken, accessToken, TimeSpan.FromHours(refreshTokenHours));

        return new AuthResponse($"Bearer {accessToken}", refreshToken);
    }
}
