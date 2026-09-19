namespace TMDEmulator.Tui;

internal sealed record MenuItem(string Label, Func<ScreenResult> Select);

/// <summary>Option menu: Up/Down to move (wrapping), Enter to select, Esc to go back (except on the root menu).</summary>
internal sealed class MenuScreen : IScreen
{
    private readonly IReadOnlyList<MenuItem> _items;
    private readonly bool _isRoot;

    public MenuScreen(string title, IReadOnlyList<MenuItem> items, bool isRoot = false)
    {
        Title = title;
        _items = items;
        _isRoot = isRoot;
    }

    public string Title { get; }

    public string Legend => _isRoot ? "↑/↓ Navigate · Enter Select" : "↑/↓ Navigate · Enter Select · Esc Back";

    public int SelectedIndex { get; private set; }

    public IReadOnlyList<MenuItem> Items => _items;

    public IReadOnlyList<BodyLine> Render()
    {
        var lines = new List<BodyLine> { BodyLine.Empty };
        for (int i = 0; i < _items.Count; i++)
        {
            bool selected = i == SelectedIndex;
            lines.Add(BodyLine.Of($"{(selected ? ">" : " ")} {i + 1}. {_items[i].Label}", selected ? Role.Highlight : Role.Normal));
        }

        return lines;
    }

    public ScreenResult HandleKey(ConsoleKeyInfo key)
    {
        switch (key.Key)
        {
            case ConsoleKey.UpArrow:
                SelectedIndex = (SelectedIndex - 1 + _items.Count) % _items.Count;
                return ScreenResult.Stay();
            case ConsoleKey.DownArrow:
                SelectedIndex = (SelectedIndex + 1) % _items.Count;
                return ScreenResult.Stay();
            case ConsoleKey.Enter:
                return _items[SelectedIndex].Select();
            case ConsoleKey.Escape:
                return _isRoot ? ScreenResult.Stay() : ScreenResult.Pop();
            default:
                return ScreenResult.Stay();
        }
    }
}
