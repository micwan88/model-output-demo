using TMDEmulator.Tests.Support;
using TMDEmulator.Tui;

namespace TMDEmulator.Tests.Tui;

public class TextInputTests
{
    [Fact]
    public void Constructor_PrefillsValueWithCaretAtEnd()
    {
        var input = new TextInput("/tmp");

        Assert.Equal("/tmp", input.Value);
        Assert.Equal(4, input.Caret);
    }

    [Fact]
    public void Typing_InsertsAtCaret()
    {
        var input = new TextInput("ac");
        input.HandleKey(Keys.Left);

        input.HandleKey(Keys.Char('b'));

        Assert.Equal("abc", input.Value);
        Assert.Equal(2, input.Caret);
    }

    [Fact]
    public void Backspace_And_Delete_RemoveAroundCaret()
    {
        var input = new TextInput("abcd");
        input.HandleKey(Keys.Left);

        input.HandleKey(Keys.Backspace); // removes 'c'
        input.HandleKey(Keys.Delete);    // removes 'd'

        Assert.Equal("ab", input.Value);
        Assert.Equal(2, input.Caret);
    }

    [Fact]
    public void Backspace_AtStart_And_Delete_AtEnd_DoNothing()
    {
        var input = new TextInput("ab");
        input.HandleKey(Keys.Delete);
        input.HandleKey(Keys.Home);
        input.HandleKey(Keys.Backspace);

        Assert.Equal("ab", input.Value);
        Assert.Equal(0, input.Caret);
    }

    [Fact]
    public void HomeEndLeftRight_MoveCaretWithinBounds()
    {
        var input = new TextInput("ab");

        input.HandleKey(Keys.Right);
        Assert.Equal(2, input.Caret);
        input.HandleKey(Keys.Home);
        input.HandleKey(Keys.Left);
        Assert.Equal(0, input.Caret);
        input.HandleKey(Keys.End);
        Assert.Equal(2, input.Caret);
    }

    [Fact]
    public void SurrogatePair_IsEditedAsOneCharacter()
    {
        var input = new TextInput("a😀b");
        input.HandleKey(Keys.Left);

        input.HandleKey(Keys.Left);
        Assert.Equal(1, input.Caret);
        input.HandleKey(Keys.Right);
        Assert.Equal(3, input.Caret);
        input.HandleKey(Keys.Backspace);
        Assert.Equal("ab", input.Value);
        input.HandleKey(Keys.Home);
        input.HandleKey(Keys.Delete);
        Assert.Equal("b", input.Value);
    }

    [Fact]
    public void Delete_SurrogatePair_RemovesBothHalves()
    {
        var input = new TextInput("😀");
        input.HandleKey(Keys.Home);

        input.HandleKey(Keys.Delete);

        Assert.Equal(string.Empty, input.Value);
    }

    [Fact]
    public void ControlCharacters_AreIgnored()
    {
        var input = new TextInput("a");

        input.HandleKey(Keys.Tab);
        input.HandleKey(Keys.Up);

        Assert.Equal("a", input.Value);
    }

    [Fact]
    public void EnterAndEscape_ReturnOutcome()
    {
        var input = new TextInput("a");

        Assert.Equal(InputOutcome.Editing, input.HandleKey(Keys.Char('b')));
        Assert.Equal(InputOutcome.Submitted, input.HandleKey(Keys.Enter));
        Assert.Equal(InputOutcome.Cancelled, input.HandleKey(Keys.Esc));
    }

    [Fact]
    public void Render_HighlightsCharacterUnderCaret()
    {
        var input = new TextInput("abc");
        input.HandleKey(Keys.Left);

        BodyLine line = input.Render();

        Assert.Equal("> abc", line.PlainText);
        Assert.Contains(line.Spans, s => s.Text == "c" && s.Role == Role.Highlight);
    }

    [Fact]
    public void Render_CaretAtEnd_ShowsHighlightedSpace()
    {
        BodyLine line = new TextInput("abc").Render();

        Assert.Equal("> abc ", line.PlainText);
        Assert.Contains(line.Spans, s => s.Text == " " && s.Role == Role.Highlight);
    }
}
