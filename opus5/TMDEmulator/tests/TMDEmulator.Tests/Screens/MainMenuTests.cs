using TMDEmulator.Crypto;
using TMDEmulator.Screens;
using TMDEmulator.Tests.Support;
using TMDEmulator.Tui;

namespace TMDEmulator.Tests.Screens;

public sealed class MainMenuTests : IDisposable
{
    private readonly TempDirectory _temp = new();

    public void Dispose() => _temp.Dispose();

    private MenuScreen Root() => MainMenu.Create(new MzmkInitExporter(FixedTimeProvider.HongKongAfternoon()), () => _temp.Path);

    private static string[] Labels(MenuScreen menu) => menu.Items.Select(i => i.Label).ToArray();

    private static MenuScreen SubMenu(MenuScreen menu, int index) => (MenuScreen)menu.Items[index].Select().Next!;

    [Fact]
    public void Root_MatchesStoryMenu()
    {
        MenuScreen root = Root();

        Assert.Equal("TMD Emulator", root.Title);
        Assert.Equal(new[] { "Remote MZMK Setup", "Key Management", "Exit" }, Labels(root));
        Assert.Equal(NavAction.Stay, root.HandleKey(Keys.Esc).Action); // root: Esc does nothing
        Assert.Equal(NavAction.Exit, root.Items[2].Select().Action);
    }

    [Fact]
    public void RemoteMzmkSetup_MatchesStoryMenu()
    {
        MenuScreen menu = SubMenu(Root(), 0);

        Assert.Equal("Remote MZMK Setup", menu.Title);
        Assert.Equal(new[] { "Export MZMK Initialization Key", "Finalize Remote MZMK", "Uninstalling a MZMK", "Back" }, Labels(menu));
        Assert.IsType<ExportMzmkInitScreen>(menu.Items[0].Select().Next);
        Assert.Equal("Finalize Remote MZMK", Assert.IsType<PlaceholderScreen>(menu.Items[1].Select().Next).Title);
        Assert.Equal("Uninstalling a MZMK", Assert.IsType<PlaceholderScreen>(menu.Items[2].Select().Next).Title);
        Assert.Equal(NavAction.Pop, menu.Items[3].Select().Action);
    }

    [Fact]
    public void KeyManagement_MatchesStoryMenu()
    {
        MenuScreen menu = SubMenu(Root(), 1);

        Assert.Equal("Key Management", menu.Title);
        Assert.Equal(new[] { "View Keys", "Back" }, Labels(menu));
        Assert.Equal("View Keys", Assert.IsType<PlaceholderScreen>(menu.Items[0].Select().Next).Title);
        Assert.Equal(NavAction.Pop, menu.Items[1].Select().Action);
    }

    [Fact]
    public void ExportScreen_UsesCurrentDirectoryAsDefault()
    {
        var export = (ExportMzmkInitScreen)SubMenu(Root(), 0).Items[0].Select().Next!;

        Assert.Equal(_temp.Path, export.Input.Value);
    }

    [Fact]
    public void EndToEnd_ExportFromStartupMenu_ReturnsToRemoteMzmkMenuWithSuccess()
    {
        var terminal = new FakeTerminal()
            .Enqueue(Keys.Enter)          // Remote MZMK Setup
            .Enqueue(Keys.Enter)          // Export MZMK Initialization Key
            .Enqueue(Keys.Enter)          // accept default directory -> save
            .Enqueue(Keys.CtrlC);
        var app = new App(terminal, Root());

        app.Run();

        string expectedPath = _temp.Combine($"MZMK_Init_{TestVectors.P256Fingerprint}20260919143005.der");
        Assert.Equal(TestVectors.P256Spki, File.ReadAllText(expectedPath));
        Assert.Equal("Remote MZMK Setup", app.Current.Title);
        Assert.Contains($"Saved: {expectedPath}", terminal.LastScreenText);
    }
}
