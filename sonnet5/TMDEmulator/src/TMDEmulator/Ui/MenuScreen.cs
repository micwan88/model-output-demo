namespace TMDEmulator.Ui;

public sealed record MenuItem(string Label, Action<ScreenHost> OnSelect);

/// <summary>An arrow-key menu: Up/Down move, Enter selects, Esc goes back.</summary>
public sealed class MenuScreen : IScreen
{
    private readonly IReadOnlyList<MenuItem> _items;
    private readonly SelectionList _list;

    public MenuScreen(string title, IReadOnlyList<MenuItem> items)
    {
        Title = title;
        _items = items;
        _list = new SelectionList(items.Select(i => i.Label).ToList());
    }

    public string Title { get; }

    public string Legend => "↑/↓ Navigate   Enter Select   Esc Back";

    public void Render(Canvas canvas) => _list.Render(canvas, 0);

    public void HandleKey(ConsoleKeyInfo key, ScreenHost host)
    {
        if (_list.HandleKey(key))
            return;

        switch (key.Key)
        {
            case ConsoleKey.Enter:
                _items[_list.Index].OnSelect(host);
                break;
            case ConsoleKey.Escape:
                host.Pop();
                break;
        }
    }
}
