namespace TMDEmulator.Tui;

/// <summary>Semantic style of a piece of text; mapped to colours by <see cref="Theme"/>.</summary>
internal enum Role
{
    Normal,
    Title,
    Border,
    Highlight,
    StatusBar,
    Error,
    Success,
}
