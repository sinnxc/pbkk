using System.Security.Cryptography;

namespace Sibumi.Core;

public static class PasswordHasher
{
    const int Iterations = 100_000;

    // Format hasil: iterasi.salt.hash (base64)
    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, 32);
        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public static bool Verify(string password, string stored)
    {
        try
        {
            var p = stored.Split('.');
            var iter = int.Parse(p[0]);
            var salt = Convert.FromBase64String(p[1]);
            var expected = Convert.FromBase64String(p[2]);
            var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iter, HashAlgorithmName.SHA256, expected.Length);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch { return false; }
    }
}
