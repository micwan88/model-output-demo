using System.Text;

namespace TMDEmulator.Crypto;

/// <summary>
/// Writes the "MZMK Initialization" file: the public key SPKI as uppercase HEX text.
/// The file uses a .der extension although its content is HEX text – this is TMD specific.
/// </summary>
internal sealed class MzmkInitExporter
{
    private readonly TimeProvider _clock;

    public MzmkInitExporter(TimeProvider clock)
    {
        _clock = clock;
    }

    /// <summary>MZMK_Init_{fingerprint}{yyyyMMddHHmmss}.der, using local time now.</summary>
    public string BuildFileName(EcKeyInfo key)
    {
        string fingerprint = KeyFingerprint.ComputeShort(key.SubjectPublicKeyInfo);
        return $"MZMK_Init_{fingerprint}{_clock.GetLocalNow():yyyyMMddHHmmss}.der";
    }

    public static string BuildContent(EcKeyInfo key) => Convert.ToHexString(key.SubjectPublicKeyInfo);

    /// <summary>
    /// Writes the file as ASCII with no trailing newline.
    /// Without <paramref name="overwrite"/>, an existing file is never replaced (IOException).
    /// </summary>
    public static void Write(string filePath, EcKeyInfo key, bool overwrite)
    {
        byte[] content = Encoding.ASCII.GetBytes(BuildContent(key));
        using var stream = new FileStream(filePath, overwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write, FileShare.None);
        stream.Write(content);
    }
}
