using System.Security.Cryptography;

namespace BusinessAssistant.Api.Services;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 32;
    private const int KeySize = 64;
    private const int Iterations = 100_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA512;

    public (string hash, string salt) Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);

        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    public bool Verify(string password, string hash, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        var hashBytes = Convert.FromBase64String(hash);

        var inputHash = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, Iterations, Algorithm, KeySize);

        return CryptographicOperations.FixedTimeEquals(inputHash, hashBytes);
    }
}
