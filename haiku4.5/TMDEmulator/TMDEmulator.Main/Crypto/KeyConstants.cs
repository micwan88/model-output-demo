namespace TMDEmulator.Main.Crypto;

public static class KeyConstants
{
    public const string CurveName = "P-256";

    public const string TestingECPrivateKeyPem = @"-----BEGIN PRIVATE KEY-----
MIGHAgEAMBMGByqGSM49AgEGCCqGSM49AwEHBG0wawIBAQQga/4T4p+rjP4JldOp
a26aefc+qWJqh7Z4i3NzjEMQZSKhRANCAARTZq5RFUplGdgysYxhzBtjS6BoqbV1
XiE4OiZT89iqCaKiu//7brYCFNJKZoYXGuQNHNX9QckJB7NSDrD0Jxwb
-----END PRIVATE KEY-----";

    public const string TestingECPublicKeyHex = "0x04ecc95d90bc96d047d2767d076aa8f0715efb8002263fdd a5c5bb9c45e5dc50558689347836849f7108d5ca7cb064de a84e0342979f403403f547c0b502917e7f8";
}
