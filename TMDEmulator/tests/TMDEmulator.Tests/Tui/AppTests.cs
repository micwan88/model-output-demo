using TMDEmulator.Screens;
using TMDEmulator.Tests.Support;
using TMDEmulator.Tui;

namespace TMDEmulator.Tests.Tui;

public class AppTests
{
    private sealed class ThrowingScreen : IScreen
    {
        public string Title => "Throwing";
        public string Legend => string.Empty;
        public IReadOnlyList<BodyLine> Render() => Array.Empty<BodyLine>();
        public ScreenResult HandleKey(ConsoleKeyInfo key) =>
            key.Key == ConsoleKey.Escape ? ScreenResult.Exit : throw new InvalidOperationException("boom");
    }

    private sealed class PopScreen : IScreen
    {
        public string Title => "Root";
        public string Legend => string.Empty;
        public IReadOnlyList<BodyLine> Render() => Array.Empty<BodyLine>();
        public ScreenResult HandleKey(ConsoleKeyInfo key) =>
            key.Key == ConsoleKey.Escape ? ScreenResult.Exit : ScreenResult.Pop();
    }

    private static MenuScreen Root() => new("TMD Emulator", new MenuItem[]
    {
        new("Page", () => ScreenResult.Push(new PlaceholderScreen("Page"))),
        new("Fail", () => ScreenResult.Stay(StatusMessage.Error("bad input"))),
        new("Exit", () => ScreenResult.Exit),
    }, isRoot: true);

    [Fact]
    public void Run_DrawsInitialScreen_ThenExits()
    {
        var terminal = new FakeTerminal().Enqueue(Keys.Up, Keys.Enter);

        new App(terminal, Root()).Run();

        Assert.Contains("TMD Emulator", terminal.FakeRows(0)[0]);
        Assert.Contains("1. Page", string.Join("\n", terminal.FakeRows(0)));
    }

    [Fact]
    public void PushAndPop_UpdateBreadcrumbAndDepth()
    {
        var terminal = new FakeTerminal().Enqueue(Keys.Enter);
        var app = new App(terminal, Root());
        terminal.Enqueue(Keys.Esc, Keys.Up, Keys.Enter);

        app.Run();

        Assert.StartsWith("╭─ TMD Emulator › Page ─", terminal.FakeRows(1)[0]);
        Assert.StartsWith("╭─ TMD Emulator ─", terminal.FakeRows(2)[0]);
        Assert.Equal(1, app.Depth);
    }

    [Fact]
    public void Message_IsShownThenClearedOnNextKey()
    {
        var terminal = new FakeTerminal().Enqueue(Keys.Down, Keys.Enter, Keys.Down, Keys.Enter);

        var app = new App(terminal, Root());
        app.Run();

        Assert.Contains("bad input", terminal.FakeRows(2)[^2]);
        Assert.DoesNotContain("bad input", terminal.FakeRows(3)[^2]);
    }

    [Fact]
    public void InitialMessage_IsShownOnFirstFrame()
    {
        var terminal = new FakeTerminal().Enqueue(Keys.CtrlC);

        new App(terminal, Root(), StatusMessage.Error("config warning")).Run();

        Assert.Contains("config warning", terminal.FakeRows(0)[^2]);
    }

    [Fact]
    public void QueuedKeys_AreHandledBeforeASingleRedraw()
    {
        var terminal = new FakeTerminal { ReportKeyAvailable = true }.Enqueue(Keys.Down, Keys.Down, Keys.Down, Keys.CtrlC);
        var root = Root();

        new App(terminal, root).Run();

        Assert.Single(terminal.Frames); // only the initial frame; the three Downs were coalesced
        Assert.Equal(0, root.SelectedIndex); // 3 items, 3 Downs -> wrapped back to first
    }

    [Fact]
    public void QueuedKeys_RedrawOnceWhenQueueDrains()
    {
        var terminal = new FakeTerminal { ReportKeyAvailable = true }.Enqueue(Keys.Down, Keys.Down).EnqueueResize(100, 30).Enqueue(Keys.CtrlC);

        new App(terminal, Root()).Run();

        Assert.Equal(3, terminal.Frames.Count); // initial, after both Downs, after resize
        Assert.Contains("> 3. Exit", string.Join("\n", terminal.FakeRows(1)));
    }

    [Fact]
    public void CtrlC_Exits()
    {
        var terminal = new FakeTerminal().Enqueue(Keys.CtrlC);

        new App(terminal, Root()).Run();

        Assert.Single(terminal.Frames);
    }

    [Fact]
    public void Resize_RedrawsAtNewSize()
    {
        var terminal = new FakeTerminal(80, 24).EnqueueResize(100, 30).Enqueue(Keys.CtrlC);

        new App(terminal, Root()).Run();

        Assert.Equal(2, terminal.Frames.Count);
        Assert.Equal(30, terminal.LastFrame.Count);
    }

    [Fact]
    public void TooSmall_ShowsNotice_AndIgnoresKeysUntilResized()
    {
        var terminal = new FakeTerminal(60, 20).Enqueue(Keys.Enter).EnqueueResize(80, 24).Enqueue(Keys.Up, Keys.Enter);
        var app = new App(terminal, Root());

        app.Run();

        Assert.StartsWith("Terminal too small", terminal.FakeRows(0)[0]);
        Assert.StartsWith("Terminal too small", terminal.FakeRows(1)[0]); // Enter ignored – no push to "Page"
        Assert.StartsWith("╭─ TMD Emulator ─", terminal.FakeRows(2)[0]);
    }

    [Fact]
    public void ScreenException_IsShownAsErrorAndAppKeepsRunning()
    {
        var terminal = new FakeTerminal().Enqueue(Keys.Enter, Keys.Esc);

        new App(terminal, new ThrowingScreen()).Run();

        Assert.Contains("Error: boom", terminal.FakeRows(1)[^2]);
        Assert.Contains(terminal.Frames[1][^2], s => s.Role == Role.Error && s.Text.Contains("boom"));
    }

    [Fact]
    public void Pop_OnRootScreen_KeepsRoot()
    {
        var terminal = new FakeTerminal().Enqueue(Keys.Enter, Keys.Esc);
        var root = new PopScreen();
        var app = new App(terminal, root);

        app.Run();

        Assert.Same(root, app.Current);
    }
}
