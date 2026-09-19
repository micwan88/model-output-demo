using TMDEmulator.Tui;

namespace TMDEmulator.Tests.Support;

/// <summary>Scripted terminal: returns queued keys (null = resize) and records every drawn frame as plain text.</summary>
internal sealed class FakeTerminal : ITerminal
{
    private readonly Queue<ConsoleKeyInfo?> _input = new();
    private readonly Queue<(int Width, int Height)> _resizes = new();

    public FakeTerminal(int width = 100, int height = 30)
    {
        Width = width;
        Height = height;
    }

    public int Width { get; set; }

    public int Height { get; set; }

    /// <summary>When true, reports queued keys as available so the app coalesces redraws.</summary>
    public bool ReportKeyAvailable { get; set; }

    public bool KeyAvailable => ReportKeyAvailable && _input.Count > 0 && _input.Peek() is not null;

    public List<IReadOnlyList<IReadOnlyList<Span>>> Frames { get; } = new();

    public IReadOnlyList<IReadOnlyList<Span>> LastFrame => Frames[^1];

    public string LastScreenText => string.Join("\n", LastFrame.Select(RowText));

    public string[] FakeRows(int frame) => Frames[frame].Select(RowText).ToArray();

    public FakeTerminal Enqueue(params ConsoleKeyInfo[] keys)
    {
        foreach (ConsoleKeyInfo key in keys)
        {
            _input.Enqueue(key);
        }

        return this;
    }

    public FakeTerminal Enqueue(IEnumerable<ConsoleKeyInfo> keys) => Enqueue(keys.ToArray());

    public FakeTerminal EnqueueResize(int width, int height)
    {
        _input.Enqueue(null);
        _resizes.Enqueue((width, height));
        return this;
    }

    public ConsoleKeyInfo? WaitForInput()
    {
        if (_input.Count == 0)
        {
            throw new InvalidOperationException("Key script exhausted before the app exited.");
        }

        ConsoleKeyInfo? next = _input.Dequeue();
        if (next is null)
        {
            (Width, Height) = _resizes.Dequeue();
        }

        return next;
    }

    public void Draw(IReadOnlyList<IReadOnlyList<Span>> rows) => Frames.Add(rows);

    public static string RowText(IReadOnlyList<Span> row) => string.Concat(row.Select(s => s.Text));
}
