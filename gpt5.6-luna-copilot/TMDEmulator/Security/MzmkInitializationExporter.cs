using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace TMDEmulator.Security;

public sealed class MzmkInitializationExporter
{
    private readonly byte[] _spkiDer;

    public MzmkInitializationExporter(string pkcs8Pem)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pkcs8Pem);

        using var ecdsa = ECDsa.Create();
        ecdsa.ImportFromPem(pkcs8Pem);
        _spkiDer = ecdsa.ExportSubjectPublicKeyInfo();
        Fingerprint = Convert.ToHexString(SHA256.HashData(_spkiDer))[..8];
        Details = CreateDetails(ecdsa);
    }

    public KeyDetails Details { get; }

    public string Fingerprint { get; }

    public string BuildFileName(DateTime timestamp)
    {
        return $"MZMK_Init_{Fingerprint}{timestamp:yyyyMMddHHmmss}.der";
    }

    public string BuildTargetPath(string outputDirectory, DateTime timestamp)
    {
        ValidateOutputDirectory(outputDirectory);
        return Path.Combine(outputDirectory, BuildFileName(timestamp));
    }

    public string Export(string outputDirectory, DateTime timestamp, bool overwrite)
    {
        var targetPath = BuildTargetPath(outputDirectory, timestamp);
        if (File.Exists(targetPath) && !overwrite)
        {
            throw new IOException($"The target file already exists: {targetPath}");
        }

        File.WriteAllText(
            targetPath,
            Convert.ToHexString(_spkiDer),
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        return targetPath;
    }

    private static KeyDetails CreateDetails(ECDsa ecdsa)
    {
        var parameters = ecdsa.ExportParameters(includePrivateParameters: true);
        var coordinateSize = parameters.Q.X?.Length
            ?? throw new CryptographicException("The EC public X coordinate is missing.");
        var x = ToFixedWidthHex(parameters.Q.X, coordinateSize);
        var y = ToFixedWidthHex(
            parameters.Q.Y
                ?? throw new CryptographicException("The EC public Y coordinate is missing."),
            coordinateSize);
        var d = ToFixedWidthHex(
            parameters.D
                ?? throw new CryptographicException("The EC private scalar is missing."),
            coordinateSize);
        var publicKeyHex = $"04{x}{y}";

        return new KeyDetails(
            ResolveCurveName(parameters.Curve),
            d,
            publicKeyHex);
    }

    private static string ResolveCurveName(ECCurve curve)
    {
        if (curve.Oid.Value == "1.2.840.10045.3.1.7")
        {
            return "secp256r1";
        }

        return curve.Oid.FriendlyName
            ?? curve.Oid.Value
            ?? "Unknown";
    }

    private static string ToFixedWidthHex(byte[] value, int width)
    {
        if (value.Length > width)
        {
            throw new CryptographicException("An EC coordinate has an invalid length.");
        }

        return Convert.ToHexString(value).PadLeft(width * 2, '0');
    }

    private static void ValidateOutputDirectory(string outputDirectory)
    {
        if (string.IsNullOrWhiteSpace(outputDirectory))
        {
            throw new ArgumentException(
                "An output directory is required.",
                nameof(outputDirectory));
        }

        if (!Directory.Exists(outputDirectory))
        {
            throw new DirectoryNotFoundException(
                $"The output directory does not exist: {outputDirectory}");
        }
    }
}
