namespace TMDEmulator.Terminal;

/// <summary>
/// An off-screen grid of colored characters. The UI draws a full frame into a buffer, then
/// <see cref="Flush"/> sends only the cells that changed since the previous frame, which avoids flicker.
/// </summary>
public sealed class ScreenBuffer
{
    public readonly record struct Cell(char Char, ConsoleColor Foreground, ConsoleColor Background);

    private readonly Cell[,] _cells;

    public ScreenBuffer(int width, int height, ConsoleColor foreground, ConsoleColor background)
    {
        Width = width;
        Height = height;
        _cells = new Cell[height, width];
        for (var r = 0; r < height; r++)
            for (var c = 0; c < width; c++)
                _cells[r, c] = new Cell(' ', foreground, background);
    }

    public int Width { get; }
    public int Height { get; }

    public Cell this[int row, int column] => _cells[row, column];

    /// <summary>Write text, silently clipping anything outside the buffer.</summary>
    public void Write(int row, int column, string text, ConsoleColor foreground, ConsoleColor background)
    {
        if (row < 0 || row >= Height)
            return;

        for (var i = 0; i < text.Length; i++)
        {
            var c = column + i;
            if (c >= 0 && c < Width)
                _cells[row, c] = new Cell(text[i], foreground, background);
        }
    }

    /// <summary>Send to the terminal what differs from <paramref name="previous"/> (everything if null).</summary>
    public void Flush(ITerminal terminal, ScreenBuffer? previous)
    {
        var comparable = previous is not null && previous.Width == Width && previous.Height == Height;

        for (var r = 0; r < Height; r++)
        {
            var c = 0;
            while (c < Width)
            {
                if (comparable && _cells[r, c] == previous![r, c])
                {
                    c++;
                    continue;
                }

                // Extend the run while cells still differ and share the same colors.
                var start = c;
                var first = _cells[r, c];
                var run = new System.Text.StringBuilder();
                while (c < Width
                       && _cells[r, c].Foreground == first.Foreground
                       && _cells[r, c].Background == first.Background
                       && (!comparable || _cells[r, c] != previous![r, c]))
                {
                    run.Append(_cells[r, c].Char);
                    c++;
                }

                terminal.Write(r, start, run.ToString(), first.Foreground, first.Background);
            }
        }
    }
}
