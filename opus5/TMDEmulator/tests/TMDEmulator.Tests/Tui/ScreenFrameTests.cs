using TMDEmulator.Tests.Support;
using TMDEmulator.Tui;

namespace TMDEmulator.Tests.Tui;

public class ScreenFrameTests
{
    private static IReadOnlyList<IReadOnlyList<Span>> Compose(
        IReadOnlyList<BodyLine>? body = null, StatusMessage? message = null, int width = 80, int height = 24, string breadcrumb = "TMD Emulator") =>
        ScreenFrame.Compose(breadcrumb, body ?? Array.Empty<BodyLine>(), "↑/↓ Navigate", message, width, height);

    [Theory]
    [InlineData(80, 24)]
    [InlineData(120, 40)]
    public void Compose_FillsEveryCellOfTheTerminal(int width, int height)
    {
        var rows = Compose(new[] { BodyLine.Of("x"), BodyLine.Of("中文路径") }, width: width, height: height);

        Assert.Equal(height, rows.Count);
        Assert.All(rows, row => Assert.Equal(width, ScreenFrame.CellCount(FakeTerminal.RowText(row))));
    }

    [Fact]
    public void Compose_HasMainFrameAboveBottomStatusFrame()
    {
        var rows = Compose(message: StatusMessage.Error("Bad path"));
        string[] text = rows.Select(FakeTerminal.RowText).ToArray();

        Assert.StartsWith("╭─ TMD Emulator ─", text[0]);
        Assert.StartsWith("╰", text[19]);                          // Main frame bottom border
        Assert.StartsWith("╭", text[20]);                          // Bottom frame top border
        Assert.StartsWith("│ ↑/↓ Navigate", text[21]);              // legend
        Assert.StartsWith("│ Bad path", text[22]);                  // message under legend
        Assert.StartsWith("╰", text[23]);
    }

    [Fact]
    public void Compose_ErrorMessage_UsesErrorRole_SuccessUsesSuccessRole()
    {
        Assert.Contains(Compose(message: StatusMessage.Error("e"))[22], s => s.Text == "e" && s.Role == Role.Error);
        Assert.Contains(Compose(message: StatusMessage.Success("ok"))[22], s => s.Text == "ok" && s.Role == Role.Success);
    }

    [Fact]
    public void Compose_HighlightedLine_FillsWholeInnerWidth()
    {
        var rows = Compose(new[] { BodyLine.Of("> 1. Item", Role.Highlight) });

        IReadOnlyList<Span> row = rows[1];
        int highlighted = row.Where(s => s.Role == Role.Highlight).Sum(s => ScreenFrame.CellCount(s.Text));
        Assert.Equal(80 - 4, highlighted);
    }

    [Fact]
    public void Compose_LongLine_WrapsWithinFrame()
    {
        string hex = new('A', 130);
        var rows = Compose(new[] { BodyLine.Of(hex) });

        Assert.Equal("│ " + new string('A', 76) + " │", FakeTerminal.RowText(rows[1]));
        Assert.StartsWith("│ " + new string('A', 54) + " ", FakeTerminal.RowText(rows[2]));
    }

    [Fact]
    public void Compose_TooManyLines_TruncatesWithEllipsis()
    {
        var body = Enumerable.Range(1, 40).Select(i => BodyLine.Of($"line {i}")).ToList();

        var rows = Compose(body);

        Assert.StartsWith("│ line 17", FakeTerminal.RowText(rows[17]));
        Assert.StartsWith("│ …", FakeTerminal.RowText(rows[18]));
    }

    [Fact]
    public void Compose_LongBreadcrumbAndMessage_AreTruncated()
    {
        var rows = Compose(message: StatusMessage.Success(new string('m', 200)), breadcrumb: new string('b', 200));

        Assert.Equal(80, ScreenFrame.CellCount(FakeTerminal.RowText(rows[0])));
        Assert.Contains("…", FakeTerminal.RowText(rows[0]));
        Assert.Contains("…", FakeTerminal.RowText(rows[22]));
    }

    [Theory]
    [InlineData(79, 24)]
    [InlineData(80, 23)]
    public void Compose_TooSmall_ShowsResizeNotice(int width, int height)
    {
        var rows = Compose(width: width, height: height);

        Assert.Equal(height, rows.Count);
        Assert.StartsWith($"Terminal too small ({width}x{height})", FakeTerminal.RowText(rows[0]));
        Assert.All(rows, row => Assert.Equal(width, ScreenFrame.CellCount(FakeTerminal.RowText(row))));
    }

    [Theory]
    [InlineData(80, 24, false)]
    [InlineData(79, 24, true)]
    [InlineData(80, 23, true)]
    [InlineData(0, 0, true)]
    public void IsTooSmall(int width, int height, bool expected)
    {
        Assert.Equal(expected, ScreenFrame.IsTooSmall(width, height));
    }

    [Theory]
    [InlineData("abc", 5, "abc")]
    [InlineData("abcdef", 4, "abc…")]
    [InlineData("中文中文", 5, "中文…")]
    [InlineData("abc", 0, "")]
    public void Truncate(string text, int max, string expected)
    {
        Assert.Equal(expected, ScreenFrame.Truncate(text, max));
    }

    [Fact]
    public void Wrap_DoesNotSplitWideCharacters()
    {
        var rows = ScreenFrame.Wrap(BodyLine.Of("ab中"), 3).Select(FakeTerminal.RowText).ToList();

        Assert.Equal(new[] { "ab ", "中 " }, rows);
    }

    [Fact]
    public void Wrap_KeepsSpanRolesAcrossRows()
    {
        var line = new BodyLine(new[] { new Span("aaaa"), new Span("X", Role.Highlight) });

        var rows = ScreenFrame.Wrap(line, 4).ToList();

        Assert.Equal(2, rows.Count);
        Assert.Contains(rows[1], s => s.Text == "X" && s.Role == Role.Highlight);
    }
}
