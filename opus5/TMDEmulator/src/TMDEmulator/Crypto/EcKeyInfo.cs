using System.Formats.Asn1;
using System.Security.Cryptography;

namespace TMDEmulator.Crypto;

/// <summary>
/// Display and export details of an EC keypair loaded from PKCS#8 PEM.
/// The curve is read from the PKCS#8 AlgorithmIdentifier, so any named curve is supported.
/// </summary>
internal sealed class EcKeyInfo
{
    private const string Pkcs8PemLabel = "PRIVATE KEY";
    private const string EcPublicKeyOid = "1.2.840.10045.2.1";

    private static readonly Dictionary<string, string> KnownCurveNames = new()
    {
        ["1.2.840.10045.3.1.7"] = "NIST P-256 (secp256r1)",
        ["1.3.132.0.34"] = "NIST P-384 (secp384r1)",
        ["1.3.132.0.35"] = "NIST P-521 (secp521r1)",
    };

    private EcKeyInfo(string curveOid, string privateKeyHex, string publicKeyHex, byte[] subjectPublicKeyInfo)
    {
        CurveOid = curveOid;
        PrivateKeyHex = privateKeyHex;
        PublicKeyHex = publicKeyHex;
        SubjectPublicKeyInfo = subjectPublicKeyInfo;
    }

    /// <summary>Named-curve OID, e.g. 1.2.840.10045.3.1.7.</summary>
    public string CurveOid { get; }

    /// <summary>Human-readable curve name, e.g. "NIST P-256 (secp256r1)".</summary>
    public string CurveName => KnownCurveNames.GetValueOrDefault(CurveOid, "Unknown curve");

    /// <summary>Curve type as shown on screen: name plus OID.</summary>
    public string CurveType => $"{CurveName} - OID {CurveOid}";

    /// <summary>Private scalar d, uppercase hex, at the curve's byte length.</summary>
    public string PrivateKeyHex { get; }

    /// <summary>Uncompressed public point 04 || X || Y, uppercase hex.</summary>
    public string PublicKeyHex { get; }

    /// <summary>SubjectPublicKeyInfo DER encoding of the public key.</summary>
    public byte[] SubjectPublicKeyInfo { get; }

    /// <exception cref="CryptographicException">The PEM is not a valid PKCS#8 EC private key.</exception>
    public static EcKeyInfo FromPkcs8Pem(string pem)
    {
        if (!PemEncoding.TryFind(pem, out PemFields fields) || pem[fields.Label] != Pkcs8PemLabel)
        {
            throw new CryptographicException($"Key is not a PKCS#8 PEM (expected '-----BEGIN {Pkcs8PemLabel}-----').");
        }

        byte[] pkcs8 = Convert.FromBase64String(pem[fields.Base64Data]);
        string curveOid = ReadCurveOid(pkcs8);

        using ECDsa ecdsa = ECDsa.Create();
        ecdsa.ImportPkcs8PrivateKey(pkcs8, out _);
        ECParameters parameters = ecdsa.ExportParameters(includePrivateParameters: true);

        // .NET exports D, Q.X and Q.Y at the curve's fixed byte length (leading zeros kept).
        string privateKeyHex = Convert.ToHexString(parameters.D!);
        string publicKeyHex = "04" + Convert.ToHexString(parameters.Q.X!) + Convert.ToHexString(parameters.Q.Y!);

        return new EcKeyInfo(curveOid, privateKeyHex, publicKeyHex, ecdsa.ExportSubjectPublicKeyInfo());
    }

    // PrivateKeyInfo ::= SEQUENCE { version INTEGER, privateKeyAlgorithm SEQUENCE { algorithm OID, parameters ANY }, ... }
    private static string ReadCurveOid(byte[] pkcs8)
    {
        try
        {
            AsnReader privateKeyInfo = new AsnReader(pkcs8, AsnEncodingRules.DER).ReadSequence();
            privateKeyInfo.ReadInteger();
            AsnReader algorithm = privateKeyInfo.ReadSequence();
            string algorithmOid = algorithm.ReadObjectIdentifier();
            if (algorithmOid != EcPublicKeyOid)
            {
                throw new CryptographicException($"Key is not an EC key (algorithm OID {algorithmOid}).");
            }

            if (algorithm.PeekTag() != Asn1Tag.ObjectIdentifier)
            {
                throw new CryptographicException("EC key does not use a named curve.");
            }

            return algorithm.ReadObjectIdentifier();
        }
        catch (AsnContentException ex)
        {
            throw new CryptographicException("Key is not a valid PKCS#8 structure.", ex);
        }
    }
}
