namespace TMDEmulator.Crypto;

/// <summary>
/// Hardcoded key material for the TMD emulator.
/// </summary>
/// <remarks>
/// TEST ONLY – never use in production. These keys are published in source control and are
/// intended solely for internal development, testing, demo and training environments.
/// </remarks>
internal static class TestKeys
{
    /// <summary>
    /// EC keypair (secp256r1 / NIST P-256) in PKCS#8 PEM format, used as the
    /// MZMK Initialization key. Generated with:
    /// <c>openssl genpkey -algorithm EC -pkeyopt ec_paramgen_curve:prime256v1</c>
    /// </summary>
    public const string MzmkInitKeyPairPem = """
        -----BEGIN PRIVATE KEY-----
        MIGHAgEAMBMGByqGSM49AgEGCCqGSM49AwEHBG0wawIBAQQgj78aviL1wTY25SUc
        qbpYEMDeRQNe+g3W/NMgbHrX5NuhRANCAAS07PgboddcqMCVvJ7CNeuyCZq0Cy4w
        o/muKZcnU9HRUuVkahmVF5RWO969/0zezPH8FiLUPaB5CsvYVcHEZ5ux
        -----END PRIVATE KEY-----
        """;
}
