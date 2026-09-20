using System.Text;
using TMDEmulator.Terminal;

namespace TMDEmulator.Tests.Support;

/// <summary>
/// In-memory terminal: records a grid of colored cells so tests can assert on what is on screen, and
/// plays back a script of keys (and side-effect actions such as a resize) to the program under test.
/// It also refuses writes that would touch the last column, which real consoles handle badly.
/// </summary>
public sealed class FakeTerminal : ITerminal
{
    private readonly Queue<object> _script = new();
    private ScreenBuffer.Cell[,] _grid = new ScreenBuffer.Cell[0, 0];

    public FakeTerminal(int width = 80, int height = 30) => Resize(width, height);

    public int Width { get; private set; }
    public int Height { get; private set; }
    public bool Initialized { get; private set; }
    public bool Restored { get; private set; }
    public int ClearCount { get; private set; }
    public List<(int Row, int Column, string Text)> Writes { get; } = [];

    public void Resize(int width, int height)
    {
        Width = width;
        Height = height;
        _grid = new ScreenBuffer.Cell[height, width];
        for (var r = 0; r < height; r++)
            for (var c = 0; c < width; c++)
                _grid[r, c] = new ScreenBuffer.Cell(' ', ConsoleColor.Gray, ConsoleColor.Black);
    }

    public void Enqueue(params ConsoleKeyInfo[] keys)
    {
        foreach (var key in keys)
            _script.Enqueue(key);
    }

    /// <summary>Run <paramref name="action"/> when the program next waits for a key (that wait reports "no key").</summary>
    public void EnqueueAction(Action action) => _script.Enqueue(action);

    public void Initialize() => Initialized = true;

    public void Restore() => Restored = true;

    public void Clear(ConsoleColor background)
    {
        ClearCount++;
        for (var r = 0; r < Height; r++)
            for (var c = 0; c < Width; c++)
                _grid[r, c] = new ScreenBuffer.Cell(' ', ConsoleColor.Gray, background);
    }

    public void Write(int row, int column, string text, ConsoleColor foreground, ConsoleColor background)
    {
        if (row < 0 || row >= Height || column < 0 || column + text.Length > Width - 1)
            throw new InvalidOperationException(
                $"Write outside the usable area: row {row}, col {column}, len {text.Length}, terminal {Width}x{Height}");

        Writes.Add((row, column, text));
        for (var i = 0; i < text.Length; i++)
            _grid[row, column + i] = new ScreenBuffer.Cell(text[i], foreground, background);
    }

    public bool WaitForKey(int timeoutMs)
    {
        if (_script.Count == 0)
            throw new InvalidOperationException("Key script exhausted but the program is still waiting for input.\n" + Dump());

        if (_script.Peek() is Action action)
        {
            _script.Dequeue();
            action();
            return false;
        }

        return true;
    }

    public ConsoleKeyInfo ReadKey() => (ConsoleKeyInfo)_script.Dequeue();

    public ScreenBuffer.Cell Cell(int row, int column) => _grid[row, column];

    public string Row(int row)
    {
        var sb = new StringBuilder(Width);
        for (var c = 0; c < Width; c++)
            sb.Append(_grid[row, c].Char);
        return sb.ToString();
    }

    /// <summary>Index of the first row containing <paramref name="text"/>; fails with a screen dump if none does.</summary>
    public int RowOf(string text)
    {
        for (var r = 0; r < Height; r++)
            if (Row(r).Contains(text, StringComparison.Ordinal))
                return r;
        throw new Xunit.Sdk.XunitException($"'{text}' not found on screen:\n{Dump()}");
    }

    public bool Shows(string text)
    {
        for (var r = 0; r < Height; r++)
            if (Row(r).Contains(text, StringComparison.Ordinal))
                return true;
        return false;
    }

    /// <summary>All rows joined with frame borders and spaces removed, so wrapped HEX reads as one string.</summary>
    public string Squashed()
    {
        var sb = new StringBuilder();
        for (var r = 0; r < Height; r++)
            sb.Append(Row(r).Replace("│", "").Replace(" ", ""));
        return sb.ToString();
    }

    public string Dump()
    {
        var sb = new StringBuilder();
        for (var r = 0; r < Height; r++)
            sb.Append(r.ToString("00")).Append('|').AppendLine(Row(r));
        return sb.ToString();
    }
}
