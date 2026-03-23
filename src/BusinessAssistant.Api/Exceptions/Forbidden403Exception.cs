namespace BusinessAssistant.Api.Exceptions;

public class Forbidden403Exception : ExceptionCustomAbstract<Forbidden403Exception>
{
    public Forbidden403Exception() { }
    public Forbidden403Exception(string message) : base(message) { }

    public static Forbidden403Exception EmailOrPassword()
        => new("Invalid email or password.");

    public static Forbidden403Exception RefreshToken()
        => new("Invalid or expired refresh token.");

    public static Forbidden403Exception TokenInvalid()
        => new("Token is invalid or has been revoked.");
}
