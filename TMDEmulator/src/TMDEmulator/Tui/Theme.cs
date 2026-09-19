using Spectre.Console;

namespace TMDEmulator.Tui;

/// <summary>Resolved colour theme. Defaults follow the story: black background, white text, grey highlight with black text.</summary>
internal sealed record Theme
{
    public static Theme Default { get; } = new();

    public Color Background { get; init; } = Color.Black;
    public Color Foreground { get; init; } = Color.White;
    public Color Border { get; init; } = Color.Grey;
    public Color HighlightBackground { get; init; } = Color.Silver;
    public Color HighlightForeground { get; init; } = Color.Black;
    public Color StatusBarBackground { get; init; } = Color.Black;
    public Color StatusBarForeground { get; init; } = Color.White;
    public Color Error { get; init; } = Color.Red;
    public Color Success { get; init; } = Color.Green;

    public Style StyleFor(Role role) => role switch
    {
        Role.Highlight => new Style(HighlightForeground, HighlightBackground),
        Role.Border => new Style(Border, Background),
        Role.Title => new Style(Foreground, Background, Decoration.Bold),
        Role.StatusBar => new Style(StatusBarForeground, StatusBarBackground),
        Role.Error => new Style(Error, StatusBarBackground),
        Role.Success => new Style(Success, StatusBarBackground),
        _ => new Style(Foreground, Background),
    };
}
