namespace BusinessAssistant.Api.DTOs;

public record RegisterRequest(string Username, string Password);

public record LoginRequest(string Username, string Password);

public record LoginResponse(string Token, DateTime ExpiresAt);
