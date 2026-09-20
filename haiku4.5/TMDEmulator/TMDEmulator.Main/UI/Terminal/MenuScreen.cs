using TMDEmulator.Main.Models;

namespace TMDEmulator.Main.UI.Terminal;

public class MenuScreen
{
    private int _currentIndex = 0;
    private List<MenuItem> _items;
    private string _title;

    public int CurrentIndex => _currentIndex;
    public List<MenuItem> Items => _items;
    public string Title => _title;

    public MenuScreen(string title, List<MenuItem> items)
    {
        _title = title;
        _items = items;
        _currentIndex = 0;
    }

    public void MoveUp()
    {
        _currentIndex = _currentIndex > 0 ? _currentIndex - 1 : _items.Count - 1;
    }

    public void MoveDown()
    {
        _currentIndex = _currentIndex < _items.Count - 1 ? _currentIndex + 1 : 0;
    }

    public MenuItem GetSelectedItem()
    {
        return _items[_currentIndex];
    }

    public int GetSelectedIndex()
    {
        return _currentIndex;
    }

    public void SetSelectedIndex(int index)
    {
        if (index >= 0 && index < _items.Count)
        {
            _currentIndex = index;
        }
    }

    public List<(string Label, bool IsSelected)> GetDisplayItems()
    {
        return _items.Select((item, index) => (item.Label, index == _currentIndex)).ToList();
    }
}
