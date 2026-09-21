using TMDEmulator.Configuration;
using TMDEmulator.Security;

namespace TMDEmulator.UI;

public sealed class TerminalRenderer
{
    private readonly ResolvedTerminalColors _colors;

    public TerminalRenderer(TerminalOptions options)
    {
        _colors = TerminalOptionsLoader.ResolveColors(options);
    }

    public void RenderMenu(MenuScreen screen, int selectedIndex, string? statusMessage, bool statusIsError)
    {
        Console.Clear();
        Apply(_colors.Foreground, _colors.Background);
        WriteBorder();
        WriteFrameLine("TMD Emulator");
        WriteFrameLine(screen.Title);
        WriteFrameLine(string.Empty);

        for (var index = 0; index < screen.Items.Count; index++)
        {
            var item = screen.Items[index];
            Apply(
                index == selectedIndex ? _colors.HighlightForeground : _colors.Foreground,
                index == selectedIndex ? _colors.HighlightBackground : _colors.Background);
            WriteFrameLine($"  {item.Label}");
        }

        Apply(_colors.FrameForeground, _colors.Background);
        WriteBorder();
        RenderStatus(statusMessage, statusIsError);
        WriteBorder();
        Console.ResetColor();
    }

    public void RenderKeyDetails(KeyDetails details, string? statusMessage, bool statusIsError)
    {
        Console.Clear();
        Apply(_colors.Foreground, _colors.Background);
        WriteBorder();
        WriteFrameLine("Export MZMK Initialization Key");
        WriteFrameLine(string.Empty);
        WriteFrameLine($"Curve type: {details.CurveType}");
        WriteFrameLine($"Private key (HEX scalar): {details.PrivateKeyHex}");
        WriteFrameLine($"Public key (HEX): {details.PublicKeyHex}");
        WriteFrameLine(string.Empty);
        Apply(_colors.FrameForeground, _colors.Background);
        WriteBorder();
        RenderStatus(statusMessage, statusIsError);
        WriteBorder();
        Console.ResetColor();
    }

    public void RenderPlaceholder(string title, string message)
    {
        Console.Clear();
        Apply(_colors.Foreground, _colors.Background);
        WriteBorder();
        WriteFrameLine(title);
        WriteFrameLine(string.Empty);
        WriteFrameLine(message);
        WriteFrameLine(string.Empty);
        Apply(_colors.FrameForeground, _colors.Background);
        WriteBorder();
        RenderStatus("Esc: Back", statusIsError: false);
        WriteBorder();
        Console.ResetColor();
    }

    private const int ContentWidth = 76;

    private void RenderStatus(string? statusMessage, bool statusIsError)
    {
        Apply(_colors.FrameForeground, _colors.Background);
        WriteFrameLine("↑/↓: Navigate   Enter: Select   Esc: Back");
        Apply(
            statusIsError ? _colors.ErrorForeground : _colors.Foreground,
            _colors.Background);
        WriteFrameLine(string.IsNullOrWhiteSpace(statusMessage) ? " " : statusMessage);
    }

    private static void WriteFrameLine(string text)
    {
        var value = text.Length > ContentWidth
            ? text[..ContentWidth]
            : text.PadRight(ContentWidth);
        Console.WriteLine($"| {value} |");
    }

    private static void WriteBorder()
    {
        Console.WriteLine($"+{new string('-', ContentWidth + 2)}+");
    }

    private static void Apply(ConsoleColor foreground, ConsoleColor background)
    {
        Console.ForegroundColor = foreground;
        Console.BackgroundColor = background;
    }
}
