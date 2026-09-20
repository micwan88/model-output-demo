using System.Security.Cryptography;

namespace TMDEmulator.Crypto;

internal static class KeyFingerprint
{
    public const int ShortLength = 8;

    /// <summary>First 8 uppercase hex characters of SHA-256(SubjectPublicKeyInfo DER).</summary>
    public static string ComputeShort(byte[] subjectPublicKeyInfo) =>
        Convert.ToHexString(SHA256.HashData(subjectPublicKeyInfo))[..ShortLength];
}
