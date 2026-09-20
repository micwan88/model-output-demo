namespace TMDEmulator.Ui;

/// <summary>A vertical list with one highlighted entry. Up/Down move the highlight and wrap around.
/// Shared by the menus and the overwrite Yes/No prompt.</summary>
public sealed class SelectionList(IReadOnlyList<string> labels)
{
    public IReadOnlyList<string> Labels { get; } = labels;
    public int Index { get; private set; }

    /// <summary>Handles Up/Down. Returns true if the key was one of them.</summary>
    public bool HandleKey(ConsoleKeyInfo key)
    {
        if (Labels.Count == 0)
            return false;

        switch (key.Key)
        {
            case ConsoleKey.UpArrow:
                Index = (Index - 1 + Labels.Count) % Labels.Count;
                return true;
            case ConsoleKey.DownArrow:
                Index = (Index + 1) % Labels.Count;
                return true;
            default:
                return false;
        }
    }

    public void Reset() => Index = 0;

    public void Render(Canvas canvas, int startRow)
    {
        for (var i = 0; i < Labels.Count; i++)
            canvas.WriteRow(startRow + i, "  " + Labels[i], i == Index ? Style.Highlight : Style.Normal);
    }
}
