using System.Security.Cryptography;
using System.Text;

namespace TMDEmulator.Main.Crypto;

public class ECKeyManager
{
    private ECDsa? _ecKey;

    public bool LoadPrivateKey(string pemPrivateKey)
    {
        try
        {
            var keyBytes = ConvertPemToBytes(pemPrivateKey);
            _ecKey = ECDsa.Create();
            _ecKey.ImportPkcs8PrivateKey(keyBytes, out _);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public string? GetPrivateKeyHex()
    {
        try
        {
            if (_ecKey == null)
                return null;

            var keyBlob = _ecKey.ExportPkcs8PrivateKey();
            var ecParameters = _ecKey.ExportParameters(true);

            if (ecParameters.D != null)
            {
                return Convert.ToHexString(ecParameters.D);
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    public string? GetPublicKeyHex()
    {
        try
        {
            if (_ecKey == null)
                return null;

            var ecParameters = _ecKey.ExportParameters(false);
            var publicKeyBytes = new List<byte> { 0x04 };

            if (ecParameters.Q.X != null)
                publicKeyBytes.AddRange(ecParameters.Q.X);
            if (ecParameters.Q.Y != null)
                publicKeyBytes.AddRange(ecParameters.Q.Y);

            return Convert.ToHexString(publicKeyBytes.ToArray());
        }
        catch
        {
            return null;
        }
    }

    public string GetCurveType()
    {
        return KeyConstants.CurveName;
    }

    public ECDsa? GetECDsa()
    {
        return _ecKey;
    }

    private static byte[] ConvertPemToBytes(string pem)
    {
        var lines = pem.Split('\n');
        var base64 = string.Concat(
            lines
                .Where(line => !line.StartsWith("-----"))
                .Select(line => line.Trim())
        );

        return Convert.FromBase64String(base64);
    }
}
