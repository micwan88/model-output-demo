using TMDEmulator.Export;
using TMDEmulator.Keys;
using TMDEmulator.Terminal;
using TMDEmulator.Ui;

namespace TMDEmulator.Tests.Support;

/// <summary>The real menu tree wired to a <see cref="FakeTerminal"/>, a frozen clock and a scratch folder.</summary>
public sealed class Harness : IDisposable
{
    public static readonly DateTimeOffset FrozenUtc = new(2026, 9, 20, 10, 5, 7, TimeSpan.Zero);
    public const string ExpectedFileName = "MZMK_Init_11D3BE2920260920100507.der";

    public Harness(int width = 80, int height = 30, IReadOnlyList<string>? startupErrors = null, Theme? theme = null)
    {
        Theme = theme ?? Theme.Default;
        Terminal = new FakeTerminal(width, height);
        Exporter = new MzmkInitExporter(EcKeyInspector.Inspect(TestKeyPair.Pkcs8Pem), new FixedTimeProvider(FrozenUtc));
        var root = MenuDefinition.CreateRoot(Exporter, () => Dir.Path);
        Host = new ScreenHost(Terminal, Theme, root, startupErrors ?? []);
        Host.Redraw();
    }

    public Theme Theme { get; }
    public FakeTerminal Terminal { get; }
    public ScreenHost Host { get; }
    public MzmkInitExporter Exporter { get; }
    public TempDirectory Dir { get; } = new();

    public string ExpectedFilePath => Dir.Combine(ExpectedFileName);

    /// <summary>Row of the key legend inside the bottom frame.</summary>
    public int LegendRow => Terminal.Height - 4;

    /// <summary>First message row (validation / error / info) directly under the legend.</summary>
    public int MessageRow => Terminal.Height - 3;

    /// <summary>Press keys the way the run loop does: handle the key, then redraw.</summary>
    public Harness Send(params ConsoleKeyInfo[] keys)
    {
        foreach (var key in keys)
        {
            Host.HandleKey(key);
            if (!Host.ExitRequested)
                Host.Redraw();
        }

        return this;
    }

    /// <summary>Main Menu -> Remote MZMK Setup -> Export MZMK Initialization Key.</summary>
    public Harness OpenExportScreen() => Send(Press.Enter, Press.Enter);

    public void Dispose() => Dir.Dispose();
}
