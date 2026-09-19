using TMDEmulator.Tui;

namespace TMDEmulator.Tests.Tui;

public class ScreenModelTests
{
    [Fact]
    public void BodyLine_Of_UsesRoleForTextAndFill()
    {
        BodyLine line = BodyLine.Of("abc", Role.Highlight);

        Assert.Equal("abc", line.PlainText);
        Assert.Equal(Role.Highlight, line.Fill);
        Assert.Equal(Role.Highlight, line.Spans[0].Role);
    }

    [Fact]
    public void BodyLine_Empty_HasNoText()
    {
        Assert.Equal(string.Empty, BodyLine.Empty.PlainText);
    }

    [Fact]
    public void StatusMessage_Factories_SetKind()
    {
        Assert.Equal(MessageKind.Error, StatusMessage.Error("x").Kind);
        Assert.Equal(MessageKind.Success, StatusMessage.Success("x").Kind);
    }

    [Fact]
    public void ScreenResult_Factories_SetAction()
    {
        var message = StatusMessage.Error("x");
        var screen = new TMDEmulator.Screens.PlaceholderScreen("p");

        Assert.Equal(new ScreenResult(NavAction.Stay, null, message), ScreenResult.Stay(message));
        Assert.Equal(new ScreenResult(NavAction.Push, screen), ScreenResult.Push(screen));
        Assert.Equal(new ScreenResult(NavAction.Pop, null, message), ScreenResult.Pop(message));
        Assert.Equal(NavAction.Exit, ScreenResult.Exit.Action);
    }
}
