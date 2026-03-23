namespace BusinessAssistant.Api.Services;

public record SaltObject(Guid Id, Guid Salt);

public interface IPasswordHasher
{
    string Hash(string password, SaltObject saltObject);
    bool Verify(string password, string hash, SaltObject saltObject);
}
