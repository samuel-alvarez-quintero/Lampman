using System.Security.Cryptography;

namespace Lampman.Core.Utils;

public static class ChecksumVerifier
{
    /// <summary>
    /// Compute checksum for a byte array with the given hash function.
    /// </summary>
    public static string Compute(byte[] data, string hashFunc)
    {
        using HashAlgorithm algo = CreateAlgorithm(hashFunc);
        var hash = algo.ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    /// <summary>
    /// Verify checksum against expected value.
    /// </summary>
    public static bool Verify(byte[] data, string hashFunc, string expected)
    {
        var actual = Compute(data, hashFunc);
        return string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Verify checksum of a file.
    /// </summary>
    public static bool VerifyFile(string filePath, string hashFunc, string expected)
    {
        using var fs = File.OpenRead(filePath);
        using HashAlgorithm algo = CreateAlgorithm(hashFunc);
        var hash = algo.ComputeHash(fs);
        var actual = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        return string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);
    }

    private static HashAlgorithm CreateAlgorithm(string hashFunc) =>
        hashFunc.ToUpperInvariant() switch
        {
            "SHA512" => SHA512.Create(),
            "SHA384" => SHA384.Create(),
            "SHA256" => SHA256.Create(),
            "SHA1" => SHA1.Create(),
            _ => throw new InvalidOperationException($"Unsupported hash algorithm: {hashFunc}")
        };
}
