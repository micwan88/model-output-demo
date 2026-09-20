namespace TMDEmulator.Tests.Support;

/// <summary>
/// Expected values for the hardcoded test keypair, produced independently with OpenSSL
/// (<c>openssl pkey -text</c> and <c>openssl pkey -pubout -outform DER</c>), not by the code under test.
/// </summary>
public static class ReferenceKey
{
    public const string PrivateScalarHex = "33319B0F27C3D25AE5F058FDA91891EE3B336926D5982808729D0454E1DB6DF4";

    public const string PublicPointHex =
        "04A43E23CBE3399CF47104638FB8F7B635CE1885BB088F3A85379B6E0060D506A422E18FA24F29132C7C4B86707FD2EC08AE307E507E23664E165ED7B082E0CEB1";

    public const string SpkiDerHex =
        "3059301306072A8648CE3D020106082A8648CE3D03010703420004A43E23CBE3399CF47104638FB8F7B635CE1885BB088F3A85379B6E0060D506A422E18FA24F29132C7C4B86707FD2EC08AE307E507E23664E165ED7B082E0CEB1";

    /// <summary>First 8 hex chars of sha256sum of the SPKI DER (11d3be29eb09...), upper-case.</summary>
    public const string Fingerprint = "11D3BE29";

    public const string CurveOid = "1.2.840.10045.3.1.7";
}
