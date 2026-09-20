namespace TMDEmulator.Keys;

/// <summary>
/// Hardcoded EC keypair used by the emulator in place of a real TMD key.
///
/// *** TEST ONLY ***  This private key is public knowledge (it is in source control). It must never be
/// used to protect real key material. It exists so the emulator can run in controlled development,
/// testing and training environments without a physical TMD.
///
/// Curve: secp256r1 (NIST P-256). The curve is read from the key itself, so replacing this value with a
/// PKCS#8 key on another named curve needs no other code change.
/// </summary>
public static class TestKeyPair
{
    public const string Pkcs8Pem = """
        -----BEGIN PRIVATE KEY-----
        MIGHAgEAMBMGByqGSM49AgEGCCqGSM49AwEHBG0wawIBAQQgMzGbDyfD0lrl8Fj9
        qRiR7jszaSbVmCgIcp0EVOHbbfShRANCAASkPiPL4zmc9HEEY4+497Y1zhiFuwiP
        OoU3m24AYNUGpCLhj6JPKRMsfEuGcH/S7AiuMH5QfiNmThZe17CC4M6x
        -----END PRIVATE KEY-----
        """;
}
