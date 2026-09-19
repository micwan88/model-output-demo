using System.Text;
using TMDEmulator.Crypto;
using TMDEmulator.Tests.Support;

namespace TMDEmulator.Tests.Crypto;

public sealed class MzmkInitExporterTests : IDisposable
{
    private readonly TempDirectory _temp = new();
    private readonly EcKeyInfo _key = EcKeyInfo.FromPkcs8Pem(TestKeys.MzmkInitKeyPairPem);

    public void Dispose() => _temp.Dispose();

    [Fact]
    public void BuildFileName_UsesFingerprintAndLocalTimestampWithoutSeparator()
    {
        var exporter = new MzmkInitExporter(FixedTimeProvider.HongKongAfternoon());

        Assert.Equal($"MZMK_Init_{TestVectors.P256Fingerprint}20260919143005.der", exporter.BuildFileName(_key));
    }

    [Fact]
    public void BuildContent_IsUppercaseSpkiHex()
    {
        Assert.Equal(TestVectors.P256Spki, MzmkInitExporter.BuildContent(_key));
    }

    [Fact]
    public void Write_CreatesAsciiHexFileWithoutTrailingNewline()
    {
        string path = _temp.Combine("out.der");

        MzmkInitExporter.Write(path, _key, overwrite: false);

        byte[] bytes = File.ReadAllBytes(path);
        Assert.Equal(TestVectors.P256Spki, Encoding.ASCII.GetString(bytes));
        Assert.Equal(TestVectors.P256Spki.Length, bytes.Length); // no BOM, no newline
    }

    [Fact]
    public void Write_ExistingFileWithoutOverwrite_ThrowsAndKeepsContent()
    {
        string path = _temp.Combine("out.der");
        File.WriteAllText(path, "original");

        Assert.Throws<IOException>(() => MzmkInitExporter.Write(path, _key, overwrite: false));
        Assert.Equal("original", File.ReadAllText(path));
    }

    [Fact]
    public void Write_ExistingFileWithOverwrite_ReplacesContent()
    {
        string path = _temp.Combine("out.der");
        File.WriteAllText(path, "original content that is longer than the hex output ....................................................................................................................................................................");

        MzmkInitExporter.Write(path, _key, overwrite: true);

        Assert.Equal(TestVectors.P256Spki, File.ReadAllText(path));
    }
}
