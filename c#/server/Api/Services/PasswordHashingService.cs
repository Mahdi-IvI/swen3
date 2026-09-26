using System.Security.Cryptography;

namespace Api.Services;

public interface IPasswordHashingService
{
    string Hash(string password);
    bool Verify(string hashedPassword, string providedPassword);
}

public sealed class PasswordHashingService : IPasswordHashingService
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA512;

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, HashSize);
        return $"{Convert.ToHexString(hash)}-{Convert.ToHexString(salt)}";
    }

    public bool Verify(string hashedPassword, string providedPassword)
    {
        var parts = hashedPassword.Split('-');
        if (parts.Length != 2) return false;
        try
        {
            var expectedHash = Convert.FromHexString(parts[0]);
            var salt = Convert.FromHexString(parts[1]);
            var actualHash = Rfc2898DeriveBytes.Pbkdf2(providedPassword, salt, Iterations, Algorithm, HashSize);
            return expectedHash.Length == HashSize && CryptographicOperations.FixedTimeEquals(expectedHash, actualHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
