using TMDEmulator.Crypto;
using TMDEmulator.Tui;

namespace TMDEmulator.Screens;

/// <summary>Builds the option menu tree shown at startup.</summary>
internal static class MainMenu
{
    public const string AppTitle = "TMD Emulator";

    public static MenuScreen Create(MzmkInitExporter exporter, Func<string> currentDirectory) =>
        new(AppTitle, new MenuItem[]
        {
            new("Remote MZMK Setup", () => ScreenResult.Push(CreateRemoteMzmkSetup(exporter, currentDirectory))),
            new("Key Management", () => ScreenResult.Push(CreateKeyManagement())),
            new("Exit", () => ScreenResult.Exit),
        }, isRoot: true);

    private static MenuScreen CreateRemoteMzmkSetup(MzmkInitExporter exporter, Func<string> currentDirectory) =>
        new("Remote MZMK Setup", new MenuItem[]
        {
            new("Export MZMK Initialization Key", () => ScreenResult.Push(new ExportMzmkInitScreen(
                EcKeyInfo.FromPkcs8Pem(TestKeys.MzmkInitKeyPairPem), exporter, currentDirectory()))),
            new("Finalize Remote MZMK", () => ScreenResult.Push(new PlaceholderScreen("Finalize Remote MZMK"))),
            new("Uninstalling a MZMK", () => ScreenResult.Push(new PlaceholderScreen("Uninstalling a MZMK"))),
            new("Back", () => ScreenResult.Pop()),
        });

    private static MenuScreen CreateKeyManagement() =>
        new("Key Management", new MenuItem[]
        {
            new("View Keys", () => ScreenResult.Push(new PlaceholderScreen("View Keys"))),
            new("Back", () => ScreenResult.Pop()),
        });
}
