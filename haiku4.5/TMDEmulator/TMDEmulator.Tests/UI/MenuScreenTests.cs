using TMDEmulator.Main.Models;
using TMDEmulator.Main.UI.Terminal;
using Xunit;

namespace TMDEmulator.Tests.UI;

public class MenuScreenTests
{
    private MenuScreen CreateTestMenuScreen()
    {
        var items = new List<MenuItem>
        {
            new("Option 1"),
            new("Option 2"),
            new("Option 3")
        };

        return new MenuScreen("Test Menu", items);
    }

    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        var menu = CreateTestMenuScreen();

        Assert.NotNull(menu);
        Assert.Equal("Test Menu", menu.Title);
        Assert.Equal(0, menu.CurrentIndex);
        Assert.Equal(3, menu.Items.Count);
    }

    [Fact]
    public void MoveDown_IncrementsIndex()
    {
        var menu = CreateTestMenuScreen();
        menu.MoveDown();

        Assert.Equal(1, menu.CurrentIndex);
    }

    [Fact]
    public void MoveDown_WrapsAroundAtEnd()
    {
        var menu = CreateTestMenuScreen();
        menu.SetSelectedIndex(2);
        menu.MoveDown();

        Assert.Equal(0, menu.CurrentIndex);
    }

    [Fact]
    public void MoveUp_DecrementsIndex()
    {
        var menu = CreateTestMenuScreen();
        menu.SetSelectedIndex(1);
        menu.MoveUp();

        Assert.Equal(0, menu.CurrentIndex);
    }

    [Fact]
    public void MoveUp_WrapsAroundAtBeginning()
    {
        var menu = CreateTestMenuScreen();
        menu.SetSelectedIndex(0);
        menu.MoveUp();

        Assert.Equal(2, menu.CurrentIndex);
    }

    [Fact]
    public void GetSelectedItem_ReturnsCorrectItem()
    {
        var menu = CreateTestMenuScreen();
        menu.SetSelectedIndex(1);

        var item = menu.GetSelectedItem();
        Assert.Equal("Option 2", item.Label);
    }

    [Fact]
    public void SetSelectedIndex_WithValidIndex_SetsCorrectly()
    {
        var menu = CreateTestMenuScreen();
        menu.SetSelectedIndex(2);

        Assert.Equal(2, menu.CurrentIndex);
    }

    [Fact]
    public void SetSelectedIndex_WithInvalidIndex_DoesNotChange()
    {
        var menu = CreateTestMenuScreen();
        menu.SetSelectedIndex(5);

        Assert.Equal(0, menu.CurrentIndex);
    }

    [Fact]
    public void GetDisplayItems_HighlightsCorrectItem()
    {
        var menu = CreateTestMenuScreen();
        menu.SetSelectedIndex(1);

        var displayItems = menu.GetDisplayItems();

        Assert.False(displayItems[0].IsSelected);
        Assert.True(displayItems[1].IsSelected);
        Assert.False(displayItems[2].IsSelected);
    }

    [Fact]
    public void GetDisplayItems_ContainsAllLabels()
    {
        var menu = CreateTestMenuScreen();

        var displayItems = menu.GetDisplayItems();

        Assert.Equal("Option 1", displayItems[0].Label);
        Assert.Equal("Option 2", displayItems[1].Label);
        Assert.Equal("Option 3", displayItems[2].Label);
    }
}
