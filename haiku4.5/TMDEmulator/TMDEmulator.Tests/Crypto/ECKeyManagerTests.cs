using TMDEmulator.Main.Crypto;
using Xunit;

namespace TMDEmulator.Tests.Crypto;

public class ECKeyManagerTests
{
    [Fact]
    public void LoadPrivateKey_WithValidPEM_ReturnsTrue()
    {
        var manager = new ECKeyManager();
        var result = manager.LoadPrivateKey(KeyConstants.TestingECPrivateKeyPem);
        Assert.True(result);
    }

    [Fact]
    public void LoadPrivateKey_WithInvalidPEM_ReturnsFalse()
    {
        var manager = new ECKeyManager();
        var result = manager.LoadPrivateKey("INVALID_PEM_DATA");
        Assert.False(result);
    }

    [Fact]
    public void GetPrivateKeyHex_AfterLoadingValidKey_ReturnsHexString()
    {
        var manager = new ECKeyManager();
        manager.LoadPrivateKey(KeyConstants.TestingECPrivateKeyPem);
        var hex = manager.GetPrivateKeyHex();
        Assert.NotNull(hex);
        Assert.NotEmpty(hex);
        Assert.Matches("^[0-9A-F]+$", hex);
    }

    [Fact]
    public void GetPublicKeyHex_AfterLoadingValidKey_ReturnsHexString()
    {
        var manager = new ECKeyManager();
        manager.LoadPrivateKey(KeyConstants.TestingECPrivateKeyPem);
        var hex = manager.GetPublicKeyHex();
        Assert.NotNull(hex);
        Assert.NotEmpty(hex);
        Assert.Matches("^[0-9A-F]+$", hex);
    }

    [Fact]
    public void GetPublicKeyHex_StartsWithUncompressedPrefix()
    {
        var manager = new ECKeyManager();
        manager.LoadPrivateKey(KeyConstants.TestingECPrivateKeyPem);
        var hex = manager.GetPublicKeyHex();
        Assert.NotNull(hex);
        Assert.StartsWith("04", hex);
    }

    [Fact]
    public void GetCurveType_ReturnsCurveName()
    {
        var manager = new ECKeyManager();
        var curveType = manager.GetCurveType();
        Assert.Equal(KeyConstants.CurveName, curveType);
    }

    [Fact]
    public void GetECDsa_AfterLoadingValidKey_ReturnsECDsaObject()
    {
        var manager = new ECKeyManager();
        manager.LoadPrivateKey(KeyConstants.TestingECPrivateKeyPem);
        var ecdsa = manager.GetECDsa();
        Assert.NotNull(ecdsa);
    }

    [Fact]
    public void GetPrivateKeyHex_BeforeLoading_ReturnsNull()
    {
        var manager = new ECKeyManager();
        var hex = manager.GetPrivateKeyHex();
        Assert.Null(hex);
    }
}
