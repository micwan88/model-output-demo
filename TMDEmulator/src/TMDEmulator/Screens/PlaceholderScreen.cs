using TMDEmulator.Tui;

namespace TMDEmulator.Screens;

/// <summary>Empty page for a function deferred to a later story.</summary>
internal sealed class PlaceholderScreen : IScreen
{
    public PlaceholderScreen(string title)
    {
        Title = title;
    }

    public string Title { get; }

    public string Legend => "Enter/Esc Back";

    public IReadOnlyList<BodyLine> Render() => new[]
    {
        BodyLine.Empty,
        BodyLine.Of(Title, Role.Title),
        BodyLine.Empty,
        BodyLine.Of("Not yet implemented - planned for a later story."),
    };

    public ScreenResult HandleKey(ConsoleKeyInfo key) =>
        key.Key is ConsoleKey.Escape or ConsoleKey.Enter ? ScreenResult.Pop() : ScreenResult.Stay();
}
