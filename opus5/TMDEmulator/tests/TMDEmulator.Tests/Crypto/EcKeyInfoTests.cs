using System.Security.Cryptography;
using TMDEmulator.Crypto;
using TMDEmulator.Tests.Support;

namespace TMDEmulator.Tests.Crypto;

public class EcKeyInfoTests
{
    public static TheoryData<string, string, string, string, string, string> KnownKeys => new()
    {
        { TestKeys.MzmkInitKeyPairPem, TestVectors.P256Oid, "NIST P-256 (secp256r1)", TestVectors.P256Private, TestVectors.P256Public, TestVectors.P256Spki },
        { TestVectors.P384Pem, TestVectors.P384Oid, "NIST P-384 (secp384r1)", TestVectors.P384Private, TestVectors.P384Public, TestVectors.P384Spki },
        { TestVectors.P521Pem, TestVectors.P521Oid, "NIST P-521 (secp521r1)", TestVectors.P521Private, TestVectors.P521Public, TestVectors.P521Spki },
    };

    [Theory]
    [MemberData(nameof(KnownKeys))]
    public void FromPkcs8Pem_MatchesOpenSslValues(string pem, string oid, string name, string privateHex, string publicHex, string spkiHex)
    {
        EcKeyInfo key = EcKeyInfo.FromPkcs8Pem(pem);

        Assert.Equal(oid, key.CurveOid);
        Assert.Equal(name, key.CurveName);
        Assert.Equal($"{name} - OID {oid}", key.CurveType);
        Assert.Equal(privateHex, key.PrivateKeyHex);
        Assert.Equal(publicHex, key.PublicKeyHex);
        Assert.Equal(spkiHex, Convert.ToHexString(key.SubjectPublicKeyInfo));
    }

    [Fact]
    public void FromPkcs8Pem_P521_KeepsLeadingZeroOfPrivateScalar()
    {
        EcKeyInfo key = EcKeyInfo.FromPkcs8Pem(TestVectors.P521Pem);

        Assert.Equal(66 * 2, key.PrivateKeyHex.Length);
        Assert.StartsWith("00", key.PrivateKeyHex);
        Assert.Equal(2 + (2 * 66 * 2), key.PublicKeyHex.Length); // "04" || X || Y, 66 bytes each
    }

    [Fact]
    public void FromPkcs8Pem_UnlistedNamedCurve_ReportsUnknownName()
    {
        using ECDsa ecdsa = ECDsa.Create(ECCurve.CreateFromValue("1.3.132.0.10")); // secp256k1
        EcKeyInfo key = EcKeyInfo.FromPkcs8Pem(ecdsa.ExportPkcs8PrivateKeyPem());

        Assert.Equal("1.3.132.0.10", key.CurveOid);
        Assert.Equal("Unknown curve", key.CurveName);
    }

    [Fact]
    public void FromPkcs8Pem_RsaKey_Throws()
    {
        using RSA rsa = RSA.Create(2048);

        var ex = Assert.Throws<CryptographicException>(() => EcKeyInfo.FromPkcs8Pem(rsa.ExportPkcs8PrivateKeyPem()));
        Assert.Contains("not an EC key", ex.Message);
    }

    [Fact]
    public void FromPkcs8Pem_Sec1EcPrivateKeyLabel_Throws()
    {
        using ECDsa ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);

        var ex = Assert.Throws<CryptographicException>(() => EcKeyInfo.FromPkcs8Pem(ecdsa.ExportECPrivateKeyPem()));
        Assert.Contains("not a PKCS#8 PEM", ex.Message);
    }

    [Fact]
    public void FromPkcs8Pem_ExplicitCurveParameters_Throws()
    {
        var ex = Assert.Throws<CryptographicException>(() => EcKeyInfo.FromPkcs8Pem(TestVectors.ExplicitCurvePem));
        Assert.Contains("named curve", ex.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not a pem")]
    public void FromPkcs8Pem_NotPem_Throws(string text)
    {
        Assert.Throws<CryptographicException>(() => EcKeyInfo.FromPkcs8Pem(text));
    }

    [Fact]
    public void FromPkcs8Pem_PemWithInvalidAsn1_Throws()
    {
        const string pem = "-----BEGIN PRIVATE KEY-----\nAQIDBA==\n-----END PRIVATE KEY-----";

        var ex = Assert.Throws<CryptographicException>(() => EcKeyInfo.FromPkcs8Pem(pem));
        Assert.Contains("not a valid PKCS#8", ex.Message);
    }
}
