using System.Security.Cryptography;
using System.Text;
using TMDEmulator.Security;

namespace TMDEmulator.Tests;

public sealed class KeyExportTests
{
    private readonly MzmkInitializationExporter _exporter =
        new(TestKeyMaterial.Pkcs8Pem);

    [Fact]
    public void KeyDetailsExposeExpectedFixtureShape()
    {
        Assert.Equal("secp256r1", _exporter.Details.CurveType);
        Assert.Equal(64, _exporter.Details.PrivateKeyHex.Length);
        Assert.Equal(130, _exporter.Details.PublicKeyHex.Length);
        Assert.StartsWith("04", _exporter.Details.PublicKeyHex);
        Assert.All(_exporter.Details.PrivateKeyHex, character =>
            Assert.Contains(character, "0123456789ABCDEF"));
        Assert.All(_exporter.Details.PublicKeyHex, character =>
            Assert.Contains(character, "0123456789ABCDEF"));
    }

    [Fact]
    public void FingerprintUsesFirstEightUppercaseSha256CharactersOfSpki()
    {
        using var ecdsa = ECDsa.Create();
        ecdsa.ImportFromPem(TestKeyMaterial.Pkcs8Pem);
        var expected = Convert.ToHexString(
            SHA256.HashData(ecdsa.ExportSubjectPublicKeyInfo()))[..8];

        Assert.Equal(expected, _exporter.Fingerprint);
    }

    [Fact]
    public void ExportWritesUppercaseSpkiHexWithoutTrailingNewline()
    {
        var directory = Directory.CreateTempSubdirectory();
        var timestamp = new DateTime(2026, 9, 21, 9, 5, 30);
        try
        {
            var path = _exporter.Export(directory.FullName, timestamp, overwrite: false);
            var content = File.ReadAllText(path, Encoding.UTF8);
            using var ecdsa = ECDsa.Create();
            ecdsa.ImportFromPem(TestKeyMaterial.Pkcs8Pem);
            var expected = Convert.ToHexString(ecdsa.ExportSubjectPublicKeyInfo());

            Assert.Equal(_exporter.BuildFileName(timestamp), Path.GetFileName(path));
            Assert.Equal(expected, content);
            Assert.DoesNotContain(Environment.NewLine, content);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Fact]
    public void ExistingFileRequiresOverwrite()
    {
        var directory = Directory.CreateTempSubdirectory();
        var timestamp = new DateTime(2026, 9, 21, 9, 5, 30);
        try
        {
            var path = _exporter.Export(directory.FullName, timestamp, overwrite: false);
            var exception = Assert.Throws<IOException>(
                () => _exporter.Export(directory.FullName, timestamp, overwrite: false));

            Assert.Equal(path, exception.Message["The target file already exists: ".Length..]);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Fact]
    public void MissingOutputDirectoryIsRejected()
    {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        Assert.Throws<DirectoryNotFoundException>(
            () => _exporter.BuildTargetPath(path, DateTime.Now));
    }
}
