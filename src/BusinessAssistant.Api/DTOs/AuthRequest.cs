namespace BusinessAssistant.Api.DTOs;

public record LoginDto(string Email, string Password);

public record SignupDto(string Email, string Password, string ConfirmPassword) : LoginDto(Email, Password);

public record RequestRefreshTokenDto(string RefreshToken);

public record AuthResponse(string AccessToken, string RefreshToken);
