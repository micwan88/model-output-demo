using System.Security.Cryptography;
using TMDEmulator.Main.Crypto;
using Xunit;

namespace TMDEmulator.Tests.Crypto;

public class KeyExporterTests
{
    [Fact]
    public void GenerateSPKIDer_WithValidECDsa_ReturnsByteArray()
    {
        var manager = new ECKeyManager();
        manager.LoadPrivateKey(KeyConstants.TestingECPrivateKeyPem);
        var ecdsa = manager.GetECDsa();

        Assert.NotNull(ecdsa);
        var der = KeyExporter.GenerateSPKIDer(ecdsa);
        Assert.NotNull(der);
        Assert.NotEmpty(der);
    }

    [Fact]
    public void ComputeFingerprint_WithValidDER_Returns8CharHexString()
    {
        var manager = new ECKeyManager();
        manager.LoadPrivateKey(KeyConstants.TestingECPrivateKeyPem);
        var ecdsa = manager.GetECDsa();
        var der = KeyExporter.GenerateSPKIDer(ecdsa);

        var fingerprint = KeyExporter.ComputeFingerprint(der);
        Assert.NotNull(fingerprint);
        Assert.Equal(8, fingerprint.Length);
        Assert.Matches("^[0-9A-F]+$", fingerprint);
    }

    [Fact]
    public void GenerateFilename_WithValidFingerprint_ReturnsCorrectFormat()
    {
        var fingerprint = "ABCD1234";
        var filename = KeyExporter.GenerateFilename(fingerprint);

        Assert.NotNull(filename);
        Assert.StartsWith("MZMK_Init_ABCD1234", filename);
        Assert.EndsWith(".der", filename);
    }

    [Fact]
    public void GenerateFilename_ContainsTimestamp()
    {
        var fingerprint = "12345678";
        var filename = KeyExporter.GenerateFilename(fingerprint);

        Assert.Matches(@"MZMK_Init_12345678\d{14}\.der", filename);
    }

    [Fact]
    public void ExportToFile_WithValidPath_CreatesFile()
    {
        var tempDir = Path.GetTempPath();
        var testHex = "0123456789ABCDEF";
        var testFilename = $"test_export_{Guid.NewGuid()}.der";

        var result = KeyExporter.ExportToFile(tempDir, testHex, testFilename);

        Assert.True(result);

        var filePath = Path.Combine(tempDir, testFilename);
        Assert.True(File.Exists(filePath));

        var content = File.ReadAllText(filePath);
        Assert.Equal(testHex, content);

        File.Delete(filePath);
    }

    [Fact]
    public void ExportToFile_WithInvalidPath_ReturnsFalse()
    {
        var invalidPath = "/nonexistent/path/12345";
        var testHex = "0123456789ABCDEF";
        var testFilename = "test.der";

        var result = KeyExporter.ExportToFile(invalidPath, testHex, testFilename);

        Assert.False(result);
    }

    [Fact]
    public void GetExportedFilePath_CombinesPathAndFilename()
    {
        var path = "/test/path";
        var filename = "test.der";

        var result = KeyExporter.GetExportedFilePath(path, filename);

        Assert.NotNull(result);
        Assert.Contains("test.der", result);
    }

    [Fact]
    public void ComputeFingerprint_IsConsistent()
    {
        var manager = new ECKeyManager();
        manager.LoadPrivateKey(KeyConstants.TestingECPrivateKeyPem);
        var ecdsa = manager.GetECDsa();
        var der = KeyExporter.GenerateSPKIDer(ecdsa);

        var fingerprint1 = KeyExporter.ComputeFingerprint(der);
        var fingerprint2 = KeyExporter.ComputeFingerprint(der);

        Assert.Equal(fingerprint1, fingerprint2);
    }
}
