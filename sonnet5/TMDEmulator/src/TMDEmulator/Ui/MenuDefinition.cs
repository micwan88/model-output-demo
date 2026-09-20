using TMDEmulator.Export;

namespace TMDEmulator.Ui;

/// <summary>The menu tree. More entries are added by later stories.</summary>
public static class MenuDefinition
{
    public static IScreen CreateRoot(MzmkInitExporter exporter, Func<string> currentDirectory) =>
        new MenuScreen("Main Menu",
        [
            new("Remote MZMK Setup", host => host.Push(CreateRemoteMzmkSetup(exporter, currentDirectory))),
            new("Key Management", host => host.Push(CreateKeyManagement())),
            new("Exit", host => host.Exit()),
        ]);

    private static IScreen CreateRemoteMzmkSetup(MzmkInitExporter exporter, Func<string> currentDirectory) =>
        new MenuScreen("Remote MZMK Setup",
        [
            new("Export MZMK Initialization Key", host => host.Push(new ExportInitKeyScreen(exporter, currentDirectory()))),
            new("Finalize Remote MZMK", host => host.Push(new PlaceholderScreen("Finalize Remote MZMK"))),
            new("Uninstalling a MZMK", host => host.Push(new PlaceholderScreen("Uninstalling a MZMK"))),
            new("Back", host => host.Pop()),
        ]);

    private static IScreen CreateKeyManagement() =>
        new MenuScreen("Key Management",
        [
            new("View Keys", host => host.Push(new PlaceholderScreen("View Keys"))),
            new("Back", host => host.Pop()),
        ]);
}
