namespace TMDEmulator.Configuration;

public sealed class TerminalOptions
{
    public TerminalColorOptions Colors { get; init; } = new();
}

public sealed class TerminalColorOptions
{
    public string Background { get; init; } = nameof(ConsoleColor.Black);
    public string Foreground { get; init; } = nameof(ConsoleColor.White);
    public string HighlightBackground { get; init; } = nameof(ConsoleColor.White);
    public string HighlightForeground { get; init; } = nameof(ConsoleColor.Black);
    public string FrameForeground { get; init; } = nameof(ConsoleColor.DarkGray);
    public string ErrorForeground { get; init; } = nameof(ConsoleColor.Red);
}

public readonly record struct ResolvedTerminalColors(
    ConsoleColor Background,
    ConsoleColor Foreground,
    ConsoleColor HighlightBackground,
    ConsoleColor HighlightForeground,
    ConsoleColor FrameForeground,
    ConsoleColor ErrorForeground);
