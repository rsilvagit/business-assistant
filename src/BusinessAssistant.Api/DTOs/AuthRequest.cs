namespace BusinessAssistant.Api.DTOs;

public record RegisterRequest(string Username, string Password);

public record LoginRequest(string Username, string Password);

public record RefreshTokenRequest(string RefreshToken);

public record LoginResponse(string Token, string RefreshToken, DateTime ExpiresAt);
