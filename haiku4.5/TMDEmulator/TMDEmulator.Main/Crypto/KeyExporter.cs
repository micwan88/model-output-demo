using System.Security.Cryptography;

namespace TMDEmulator.Main.Crypto;

public class KeyExporter
{
    public static byte[] GenerateSPKIDer(ECDsa publicKey)
    {
        try
        {
            return publicKey.ExportSubjectPublicKeyInfo();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to generate SPKI DER", ex);
        }
    }

    public static string ComputeFingerprint(byte[] spiDerBytes)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(spiDerBytes);
        var hashHex = Convert.ToHexString(hashBytes);
        return hashHex.Substring(0, 8);
    }

    public static string GenerateFilename(string fingerprint)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        return $"MZMK_Init_{fingerprint}{timestamp}.der";
    }

    public static bool ExportToFile(string outputPath, string spiHex, string filename)
    {
        try
        {
            if (!Directory.Exists(outputPath))
            {
                return false;
            }

            var filePath = Path.Combine(outputPath, filename);
            File.WriteAllText(filePath, spiHex);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static string GetExportedFilePath(string outputPath, string filename)
    {
        return Path.Combine(outputPath, filename);
    }
}
