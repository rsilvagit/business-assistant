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
    private readonly IConfiguration _configuration;

    public AuthService(
        AppDbContext context,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IConfiguration configuration)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
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

        var token = _tokenService.GenerateToken(user);
        var expiration = DateTime.UtcNow.AddHours(
            double.Parse(_configuration["Jwt:ExpirationInHours"]!));

        return new LoginResponse(token, expiration);
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash, user.PasswordSalt))
            throw new UnauthorizedAccessException("Invalid username or password.");

        var token = _tokenService.GenerateToken(user);
        var expiration = DateTime.UtcNow.AddHours(
            double.Parse(_configuration["Jwt:ExpirationInHours"]!));

        return new LoginResponse(token, expiration);
    }
}
