namespace TMDEmulator.Ui;

/// <summary>An empty page for a function that a later story will implement.</summary>
public sealed class PlaceholderScreen(string title) : IScreen
{
    public string Title { get; } = title;

    public string Legend => "Esc Back";

    public void Render(Canvas canvas) =>
        canvas.Write(0, 0, "Not implemented yet - planned for a later story.");

    public void HandleKey(ConsoleKeyInfo key, ScreenHost host)
    {
        if (key.Key == ConsoleKey.Escape)
            host.Pop();
    }
}
