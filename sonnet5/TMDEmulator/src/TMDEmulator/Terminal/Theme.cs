namespace TMDEmulator.Terminal;

/// <summary>The colors the UI is drawn with. Defaults follow the story: black background, white text,
/// grey highlight row with black text, red errors.</summary>
public sealed record Theme(
    ConsoleColor Background,
    ConsoleColor Text,
    ConsoleColor HighlightBackground,
    ConsoleColor HighlightText,
    ConsoleColor Error)
{
    public static Theme Default { get; } = new(
        Background: ConsoleColor.Black,
        Text: ConsoleColor.White,
        HighlightBackground: ConsoleColor.Gray,
        HighlightText: ConsoleColor.Black,
        Error: ConsoleColor.Red);
}
