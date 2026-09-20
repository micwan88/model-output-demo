using TMDEmulator.Tests.Support;
using TMDEmulator.Tui;

namespace TMDEmulator.Tests.Tui;

public class ConfirmPromptTests
{
    [Fact]
    public void DefaultsToNo()
    {
        var prompt = new ConfirmPrompt();

        Assert.False(prompt.YesSelected);
        Assert.Equal(ConfirmOutcome.No, prompt.HandleKey(Keys.Enter));
    }

    [Theory]
    [InlineData(ConsoleKey.LeftArrow)]
    [InlineData(ConsoleKey.RightArrow)]
    [InlineData(ConsoleKey.UpArrow)]
    [InlineData(ConsoleKey.DownArrow)]
    [InlineData(ConsoleKey.Tab)]
    public void NavigationKeys_ToggleSelection(ConsoleKey key)
    {
        var prompt = new ConfirmPrompt();

        Assert.Equal(ConfirmOutcome.Pending, prompt.HandleKey(Keys.Of(key)));
        Assert.True(prompt.YesSelected);
        Assert.Equal(ConfirmOutcome.Yes, prompt.HandleKey(Keys.Enter));
    }

    [Fact]
    public void Escape_MeansNo_EvenWhenYesSelected()
    {
        var prompt = new ConfirmPrompt();
        prompt.HandleKey(Keys.Left);

        Assert.Equal(ConfirmOutcome.No, prompt.HandleKey(Keys.Esc));
    }

    [Fact]
    public void OtherKeys_ArePending()
    {
        Assert.Equal(ConfirmOutcome.Pending, new ConfirmPrompt().HandleKey(Keys.Char('y')));
    }

    [Fact]
    public void Render_HighlightsSelectedOption()
    {
        var prompt = new ConfirmPrompt();
        Assert.Contains(prompt.Render().Spans, s => s.Text.Trim() == "No" && s.Role == Role.Highlight);

        prompt.HandleKey(Keys.Left);
        Assert.Contains(prompt.Render().Spans, s => s.Text.Trim() == "Yes" && s.Role == Role.Highlight);
        Assert.Contains(prompt.Render().Spans, s => s.Text.Trim() == "No" && s.Role == Role.Normal);
    }
}
