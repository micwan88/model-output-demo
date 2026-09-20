using TMDEmulator.Terminal;

namespace TMDEmulator.Ui;

public enum Style
{
    Normal,
    Highlight,
    Error,
}

/// <summary>A rectangular region of a <see cref="ScreenBuffer"/>. Screens draw with region-relative
/// coordinates and semantic <see cref="Style"/>s; the theme decides the actual colors.</summary>
public sealed class Canvas(ScreenBuffer buffer, Theme theme, int top, int left, int width, int height)
{
    public int Width { get; } = width;
    public int Height { get; } = height;

    public Canvas Sub(int top1, int left1, int width1, int height1) =>
        new(buffer, theme, top + top1, left + left1, width1, height1);

    /// <summary>Write text, clipped to this region.</summary>
    public void Write(int row, int column, string text, Style style = Style.Normal)
    {
        if (row < 0 || row >= Height || column >= Width)
            return;
        if (column < 0)
        {
            text = text.Length > -column ? text[-column..] : string.Empty;
            column = 0;
        }

        if (text.Length > Width - column)
            text = text[..(Width - column)];

        var (fg, bg) = Colors(style);
        buffer.Write(top + row, left + column, text, fg, bg);
    }

    /// <summary>Write text and pad with spaces to the full region width (used for highlight rows).</summary>
    public void WriteRow(int row, string text, Style style = Style.Normal) =>
        Write(row, 0, text.PadRight(Width), style);

    private (ConsoleColor Foreground, ConsoleColor Background) Colors(Style style) => style switch
    {
        Style.Highlight => (theme.HighlightText, theme.HighlightBackground),
        Style.Error => (theme.Error, theme.Background),
        _ => (theme.Text, theme.Background),
    };
}
