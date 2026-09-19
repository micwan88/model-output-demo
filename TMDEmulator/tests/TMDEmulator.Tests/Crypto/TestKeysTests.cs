using TMDEmulator.Crypto;
using TMDEmulator.Tests.Support;

namespace TMDEmulator.Tests.Crypto;

public class TestKeysTests
{
    [Fact]
    public void MzmkInitKeyPairPem_IsPkcs8Pem()
    {
        Assert.StartsWith("-----BEGIN PRIVATE KEY-----", TestKeys.MzmkInitKeyPairPem);
        Assert.EndsWith("-----END PRIVATE KEY-----", TestKeys.MzmkInitKeyPairPem.TrimEnd());
    }

    [Fact]
    public void MzmkInitKeyPairPem_IsSecp256r1()
    {
        EcKeyInfo key = EcKeyInfo.FromPkcs8Pem(TestKeys.MzmkInitKeyPairPem);

        Assert.Equal(TestVectors.P256Oid, key.CurveOid);
    }
}
