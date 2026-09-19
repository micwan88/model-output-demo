using TMDEmulator.Crypto;
using TMDEmulator.Tests.Support;

namespace TMDEmulator.Tests.Crypto;

public class KeyFingerprintTests
{
    [Theory]
    [InlineData(TestVectors.P256Spki, TestVectors.P256Fingerprint)]
    [InlineData(TestVectors.P384Spki, TestVectors.P384Fingerprint)]
    [InlineData(TestVectors.P521Spki, TestVectors.P521Fingerprint)]
    public void ComputeShort_MatchesSha256sumOfSpki(string spkiHex, string expected)
    {
        Assert.Equal(expected, KeyFingerprint.ComputeShort(Convert.FromHexString(spkiHex)));
    }

    [Fact]
    public void ComputeShort_IsEightUppercaseHexChars()
    {
        // SHA-256("") = E3B0C442...
        Assert.Equal("E3B0C442", KeyFingerprint.ComputeShort(Array.Empty<byte>()));
    }
}
