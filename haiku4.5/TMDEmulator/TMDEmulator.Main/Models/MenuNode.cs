namespace TMDEmulator.Main.Models;

public class MenuNode
{
    public string Name { get; set; }
    public List<MenuItem> Items { get; set; }
    public Dictionary<int, MenuNode>? Children { get; set; }

    public MenuNode(string name, List<MenuItem> items, Dictionary<int, MenuNode>? children = null)
    {
        Name = name;
        Items = items;
        Children = children ?? new Dictionary<int, MenuNode>();
    }
}
