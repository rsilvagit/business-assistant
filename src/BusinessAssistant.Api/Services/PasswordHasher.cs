using System.Security.Cryptography;
using System.Text;

namespace BusinessAssistant.Api.Services;

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password, SaltObject saltObject)
    {
        var input = $"{password}:{saltObject.Id}:{saltObject.Salt}";
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(bytes);
    }

    public bool Verify(string password, string hash, SaltObject saltObject)
    {
        var computedHash = Hash(password, saltObject);
        return string.Equals(computedHash, hash, StringComparison.Ordinal);
    }
}
