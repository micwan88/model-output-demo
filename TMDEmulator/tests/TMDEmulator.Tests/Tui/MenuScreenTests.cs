using TMDEmulator.Tests.Support;
using TMDEmulator.Tui;

namespace TMDEmulator.Tests.Tui;

public class MenuScreenTests
{
    private static MenuScreen Menu(bool isRoot = false, List<string>? selected = null) => new("Menu", new MenuItem[]
    {
        new("First", () => { selected?.Add("First"); return ScreenResult.Stay(); }),
        new("Second", () => { selected?.Add("Second"); return ScreenResult.Exit; }),
        new("Third", () => ScreenResult.Pop()),
    }, isRoot);

    [Fact]
    public void Render_NumbersItemsAndHighlightsSelection()
    {
        IReadOnlyList<BodyLine> lines = Menu().Render();

        Assert.Equal("> 1. First", lines[1].PlainText);
        Assert.Equal(Role.Highlight, lines[1].Fill);
        Assert.Equal("  2. Second", lines[2].PlainText);
        Assert.Equal(Role.Normal, lines[2].Fill);
    }

    [Fact]
    public void UpDown_MoveSelectionAndWrap()
    {
        MenuScreen menu = Menu();

        menu.HandleKey(Keys.Up);
        Assert.Equal(2, menu.SelectedIndex);
        menu.HandleKey(Keys.Down);
        Assert.Equal(0, menu.SelectedIndex);
        menu.HandleKey(Keys.Down);
        Assert.Equal(1, menu.SelectedIndex);
    }

    [Fact]
    public void Enter_InvokesSelectedItem()
    {
        var selected = new List<string>();
        MenuScreen menu = Menu(selected: selected);
        menu.HandleKey(Keys.Down);

        ScreenResult result = menu.HandleKey(Keys.Enter);

        Assert.Equal(new[] { "Second" }, selected);
        Assert.Equal(NavAction.Exit, result.Action);
    }

    [Fact]
    public void Escape_PopsSubMenu_ButNotRootMenu()
    {
        Assert.Equal(NavAction.Pop, Menu().HandleKey(Keys.Esc).Action);
        Assert.Equal(NavAction.Stay, Menu(isRoot: true).HandleKey(Keys.Esc).Action);
    }

    [Fact]
    public void Legend_OmitsEscOnRoot()
    {
        Assert.Equal("↑/↓ Navigate · Enter Select · Esc Back", Menu().Legend);
        Assert.Equal("↑/↓ Navigate · Enter Select", Menu(isRoot: true).Legend);
    }

    [Fact]
    public void OtherKeys_AreIgnored()
    {
        MenuScreen menu = Menu();

        Assert.Equal(NavAction.Stay, menu.HandleKey(Keys.Char('x')).Action);
        Assert.Equal(0, menu.SelectedIndex);
    }
}
