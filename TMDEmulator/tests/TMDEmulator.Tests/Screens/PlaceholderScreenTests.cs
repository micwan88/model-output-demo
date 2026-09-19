using TMDEmulator.Screens;
using TMDEmulator.Tests.Support;
using TMDEmulator.Tui;

namespace TMDEmulator.Tests.Screens;

public class PlaceholderScreenTests
{
    [Fact]
    public void Render_ShowsTitleAndPlaceholderText()
    {
        var screen = new PlaceholderScreen("View Keys");

        string[] text = screen.Render().Select(l => l.PlainText).ToArray();

        Assert.Equal("View Keys", screen.Title);
        Assert.Contains("View Keys", text);
        Assert.Contains("Not yet implemented - planned for a later story.", text);
    }

    [Theory]
    [InlineData(ConsoleKey.Escape, true)]
    [InlineData(ConsoleKey.Enter, true)]
    [InlineData(ConsoleKey.DownArrow, false)]
    public void Keys_EscOrEnterGoBack(ConsoleKey key, bool expectBack)
    {
        Assert.Equal(expectBack ? NavAction.Pop : NavAction.Stay, new PlaceholderScreen("x").HandleKey(Keys.Of(key)).Action);
    }

    [Fact]
    public void Legend_DescribesBack()
    {
        Assert.Equal("Enter/Esc Back", new PlaceholderScreen("x").Legend);
    }
}
