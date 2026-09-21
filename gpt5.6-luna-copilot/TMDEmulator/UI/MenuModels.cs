namespace TMDEmulator.UI;

public enum MenuAction
{
    None,
    ExportInitializationKey,
    Placeholder,
    Back,
    Exit
}

public sealed record MenuItem(string Label, MenuAction Action = MenuAction.None);

public sealed record MenuScreen(string Title, IReadOnlyList<MenuItem> Items);

public static class MenuDefinitions
{
    public static MenuScreen CreateRoot() => new(
        "TMD Emulator",
        new[]
        {
            new MenuItem("Remote MZMK Setup"),
            new MenuItem("Key Management"),
            new MenuItem("Exit", MenuAction.Exit)
        });

    public static MenuScreen CreateRemoteMzmkSetup() => new(
        "Remote MZMK Setup",
        new[]
        {
            new MenuItem("Export MZMK Initialization Key", MenuAction.ExportInitializationKey),
            new MenuItem("Finalize Remote MZMK", MenuAction.Placeholder),
            new MenuItem("Uninstalling a MZMK", MenuAction.Placeholder),
            new MenuItem("Back", MenuAction.Back)
        });

    public static MenuScreen CreateKeyManagement() => new(
        "Key Management",
        new[]
        {
            new MenuItem("View Keys", MenuAction.Placeholder),
            new MenuItem("Back", MenuAction.Back)
        });

    public static MenuScreen CreatePlaceholder(string title) => new(
        title,
        new[] { new MenuItem("Back", MenuAction.Back) });
}

public sealed class MenuNavigator
{
    private readonly Stack<NavigationFrame> _frames = new();

    public MenuNavigator(MenuScreen root)
    {
        ArgumentNullException.ThrowIfNull(root);
        ValidateScreen(root);
        _frames.Push(new NavigationFrame(root));
    }

    public MenuScreen CurrentScreen => _frames.Peek().Screen;

    public int SelectedIndex => _frames.Peek().SelectedIndex;

    public void MoveSelection(int direction)
    {
        var frame = _frames.Peek();
        frame.SelectedIndex = (frame.SelectedIndex + direction) % frame.Screen.Items.Count;
        if (frame.SelectedIndex < 0)
        {
            frame.SelectedIndex += frame.Screen.Items.Count;
        }
    }

    public MenuItem Activate() => CurrentScreen.Items[SelectedIndex];

    public void Push(MenuScreen screen)
    {
        ArgumentNullException.ThrowIfNull(screen);
        ValidateScreen(screen);
        _frames.Push(new NavigationFrame(screen));
    }

    public bool GoBack()
    {
        if (_frames.Count == 1)
        {
            return false;
        }

        _frames.Pop();
        return true;
    }

    private static void ValidateScreen(MenuScreen screen)
    {
        if (screen.Items.Count == 0)
        {
            throw new ArgumentException("A menu screen must contain an item.", nameof(screen));
        }
    }

    private sealed class NavigationFrame(MenuScreen screen)
    {
        public MenuScreen Screen { get; } = screen;
        public int SelectedIndex { get; set; }
    }
}
