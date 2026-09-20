using System.Security.Cryptography;

namespace TMDEmulator.Keys;

/// <summary>Displayable facts about an EC keypair. All hex is upper-case.</summary>
/// <param name="CurveName">Well-known curve name, e.g. secp256r1.</param>
/// <param name="CurveOid">Curve OID, e.g. 1.2.840.10045.3.1.7 (empty if the platform did not report one).</param>
/// <param name="PrivateKeyHex">Private scalar D.</param>
/// <param name="PublicKeyHex">Uncompressed public point, 04 || X || Y.</param>
/// <param name="Fingerprint">First 8 hex chars of SHA-256 over the SubjectPublicKeyInfo DER.</param>
public sealed record EcKeyInfo(
    string CurveName,
    string CurveOid,
    string PrivateKeyHex,
    string PublicKeyHex,
    string Fingerprint);

public static class EcKeyInspector
{
    private const int FingerprintLength = 8;

    // Keyed by both OID and .NET friendly name because platforms differ in which one they report.
    private static readonly Dictionary<string, string> CurveNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["1.2.840.10045.3.1.7"] = "secp256r1",
        ["nistP256"] = "secp256r1",
        ["ECDSA_P256"] = "secp256r1",
        ["1.3.132.0.34"] = "secp384r1",
        ["nistP384"] = "secp384r1",
        ["ECDSA_P384"] = "secp384r1",
        ["1.3.132.0.35"] = "secp521r1",
        ["nistP521"] = "secp521r1",
        ["ECDSA_P521"] = "secp521r1",
    };

    /// <exception cref="CryptographicException">The PEM is not a valid PKCS#8 EC private key.</exception>
    /// <exception cref="NotSupportedException">The key does not use a named curve.</exception>
    public static EcKeyInfo Inspect(string pkcs8Pem)
    {
        using var ecdsa = ECDsa.Create();
        ecdsa.ImportFromPem(pkcs8Pem);

        var p = ecdsa.ExportParameters(includePrivateParameters: true);
        if (!p.Curve.IsNamed)
            throw new NotSupportedException("Only keys on a named curve are supported.");

        var point = new byte[1 + p.Q.X!.Length + p.Q.Y!.Length];
        point[0] = 0x04;
        p.Q.X.CopyTo(point, 1);
        p.Q.Y.CopyTo(point, 1 + p.Q.X.Length);

        var spki = ecdsa.ExportSubjectPublicKeyInfo();
        var fingerprint = Convert.ToHexString(SHA256.HashData(spki))[..FingerprintLength];

        var oid = p.Curve.Oid.Value ?? string.Empty;
        return new EcKeyInfo(
            CurveName: DescribeCurve(p.Curve.Oid),
            CurveOid: oid,
            PrivateKeyHex: Convert.ToHexString(p.D!),
            PublicKeyHex: Convert.ToHexString(point),
            Fingerprint: fingerprint);
    }

    private static string DescribeCurve(Oid oid)
    {
        if (oid.Value is { } value && CurveNames.TryGetValue(value, out var byOid))
            return byOid;
        if (oid.FriendlyName is { } friendly)
            return CurveNames.GetValueOrDefault(friendly, friendly);
        return oid.Value ?? "unknown";
    }
}
