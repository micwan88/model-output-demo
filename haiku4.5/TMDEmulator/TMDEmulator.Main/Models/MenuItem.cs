namespace TMDEmulator.Main.Models;

public class MenuItem
{
    public string Label { get; set; }
    public string? Description { get; set; }
    public Action? OnSelect { get; set; }

    public MenuItem(string label, string? description = null, Action? onSelect = null)
    {
        Label = label;
        Description = description;
        OnSelect = onSelect;
    }
}
