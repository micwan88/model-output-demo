using System.Security.Cryptography;
using TMDEmulator.Keys;
using TMDEmulator.Tests.Support;

namespace TMDEmulator.Tests;

public sealed class EcKeyInspectorTests
{
    private static readonly EcKeyInfo Info = EcKeyInspector.Inspect(TestKeyPair.Pkcs8Pem);

    [Fact]
    public void Hardcoded_keypair_is_secp256r1()
    {
        Assert.Equal("secp256r1", Info.CurveName);
        Assert.Equal(ReferenceKey.CurveOid, Info.CurveOid);
    }

    [Fact]
    public void Private_scalar_matches_openssl()
    {
        Assert.Equal(ReferenceKey.PrivateScalarHex, Info.PrivateKeyHex);
    }

    [Fact]
    public void Public_key_is_the_uncompressed_point_04_X_Y_in_upper_case_hex()
    {
        Assert.Equal(ReferenceKey.PublicPointHex, Info.PublicKeyHex);
        Assert.StartsWith("04", Info.PublicKeyHex);
        Assert.Equal(130, Info.PublicKeyHex.Length);
        Assert.Equal(Info.PublicKeyHex.ToUpperInvariant(), Info.PublicKeyHex);
    }

    [Fact]
    public void Fingerprint_is_first_8_hex_chars_of_sha256_over_the_spki_der()
    {
        // Independent path: hash the SPKI bytes produced by OpenSSL, not the ones .NET exports.
        var expected = Convert.ToHexString(SHA256.HashData(Convert.FromHexString(ReferenceKey.SpkiDerHex)))[..8];

        Assert.Equal(expected, Info.Fingerprint);
        Assert.Equal(ReferenceKey.Fingerprint, Info.Fingerprint);
        Assert.Equal(8, Info.Fingerprint.Length);
    }

    [Fact]
    public void The_public_point_really_belongs_to_the_private_scalar()
    {
        // Sign with the private key, verify with a key rebuilt only from the reported public point.
        using var priv = ECDsa.Create();
        priv.ImportFromPem(TestKeyPair.Pkcs8Pem);
        var data = new byte[] { 1, 2, 3 };
        var signature = priv.SignData(data, HashAlgorithmName.SHA256);

        var point = Convert.FromHexString(Info.PublicKeyHex);
        using var pub = ECDsa.Create(new ECParameters
        {
            Curve = ECCurve.NamedCurves.nistP256,
            Q = new ECPoint { X = point[1..33], Y = point[33..] },
        });

        Assert.True(pub.VerifyData(data, signature, HashAlgorithmName.SHA256));
    }

    [Fact]
    public void Curve_is_read_from_the_key_not_hardcoded()
    {
        using var p384 = ECDsa.Create(ECCurve.NamedCurves.nistP384);
        var info = EcKeyInspector.Inspect(p384.ExportPkcs8PrivateKeyPem());

        Assert.Equal("secp384r1", info.CurveName);
        Assert.Equal("1.3.132.0.34", info.CurveOid);
        Assert.Equal(96, info.PrivateKeyHex.Length);
        Assert.Equal(2 + 96 * 2, info.PublicKeyHex.Length);
    }

    [Fact]
    public void P521_key_is_named_secp521r1()
    {
        using var p521 = ECDsa.Create(ECCurve.NamedCurves.nistP521);

        Assert.Equal("secp521r1", EcKeyInspector.Inspect(p521.ExportPkcs8PrivateKeyPem()).CurveName);
    }

    [Fact]
    public void A_curve_without_a_secpXXXr1_name_is_reported_under_its_own_name()
    {
        using var brainpool = ECDsa.Create(ECCurve.NamedCurves.brainpoolP256r1);

        var info = EcKeyInspector.Inspect(brainpool.ExportPkcs8PrivateKeyPem());

        Assert.Equal("brainpoolP256r1", info.CurveName);
        Assert.Equal("1.3.36.3.3.2.8.1.1.7", info.CurveOid);
    }

    // P-256 written with its full domain parameters instead of a curve OID (openssl ec_param_enc:explicit).
    private const string ExplicitParametersPem = """
        -----BEGIN PRIVATE KEY-----
        MIIBeQIBADCCAQMGByqGSM49AgEwgfcCAQEwLAYHKoZIzj0BAQIhAP////8AAAAB
        AAAAAAAAAAAAAAAA////////////////MFsEIP////8AAAABAAAAAAAAAAAAAAAA
        ///////////////8BCBaxjXYqjqT57PrvVV2mIa8ZR0GsMxTsPY7zjw+J9JgSwMV
        AMSdNgiG5wSTamZ44ROdJreBn36QBEEEaxfR8uEsQkf4vOblY6RA8ncDfYEt6zOg
        9KE5RdiYwpZP40Li/hp/m47n60p8D54WK84zV2sxXs7LtkBoN79R9QIhAP////8A
        AAAA//////////+85vqtpxeehPO5ysL8YyVRAgEBBG0wawIBAQQgH19t6VaamYhZ
        BFAeVkaAYJsr13ZNe7LIGuwyF/TKyEWhRANCAASgRnSi7NlDtGTS+l5VZz/4pErR
        gCbMgpaRp5SV81cuF5mxKG0ODmEjLeqjmpF3ShQ1vitu4uvlcpvGTkXqQZ9e
        -----END PRIVATE KEY-----
        """;

    [Fact]
    public void A_key_that_carries_explicit_curve_parameters_is_rejected_with_a_clear_message()
    {
        var ex = Assert.Throws<NotSupportedException>(() => EcKeyInspector.Inspect(ExplicitParametersPem));

        Assert.Contains("named curve", ex.Message);
    }

    [Fact]
    public void Text_that_is_not_a_pem_is_rejected()
    {
        Assert.ThrowsAny<ArgumentException>(() => EcKeyInspector.Inspect("not a pem at all"));
    }

    [Fact]
    public void A_pem_with_garbage_content_is_rejected()
    {
        const string bad = "-----BEGIN PRIVATE KEY-----\nAAAA\n-----END PRIVATE KEY-----";

        Assert.ThrowsAny<CryptographicException>(() => EcKeyInspector.Inspect(bad));
    }

    [Fact]
    public void An_rsa_key_is_rejected()
    {
        using var rsa = RSA.Create(2048);

        Assert.ThrowsAny<Exception>(() => EcKeyInspector.Inspect(rsa.ExportPkcs8PrivateKeyPem()));
    }
}
