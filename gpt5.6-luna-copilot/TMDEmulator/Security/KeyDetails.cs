namespace TMDEmulator.Security;

public sealed record KeyDetails(
    string CurveType,
    string PrivateKeyHex,
    string PublicKeyHex);
