using TMDEmulator.Terminal;
using TMDEmulator.Tests.Support;
using TMDEmulator.Ui;

namespace TMDEmulator.Tests;

public sealed class CanvasAndBufferTests
{
    private static readonly Theme Custom = new(
        Background: ConsoleColor.DarkBlue,
        Text: ConsoleColor.Yellow,
        HighlightBackground: ConsoleColor.Cyan,
        HighlightText: ConsoleColor.DarkRed,
        Error: ConsoleColor.Magenta);

    private static ScreenBuffer NewBuffer(int w = 20, int h = 5) =>
        new(w, h, Custom.Text, Custom.Background);

    private static string RowText(ScreenBuffer b, int row) =>
        new(Enumerable.Range(0, b.Width).Select(c => b[row, c].Char).ToArray());

    // ---- Canvas -------------------------------------------------------------------------------

    [Fact]
    public void Canvas_maps_styles_to_theme_colors()
    {
        var buffer = NewBuffer();
        var canvas = new Canvas(buffer, Custom, 0, 0, 20, 5);

        canvas.Write(0, 0, "a", Style.Normal);
        canvas.Write(1, 0, "b", Style.Highlight);
        canvas.Write(2, 0, "c", Style.Error);

        Assert.Equal((ConsoleColor.Yellow, ConsoleColor.DarkBlue), (buffer[0, 0].Foreground, buffer[0, 0].Background));
        Assert.Equal((ConsoleColor.DarkRed, ConsoleColor.Cyan), (buffer[1, 0].Foreground, buffer[1, 0].Background));
        Assert.Equal((ConsoleColor.Magenta, ConsoleColor.DarkBlue), (buffer[2, 0].Foreground, buffer[2, 0].Background));
    }

    [Fact]
    public void Canvas_clips_text_at_its_right_edge()
    {
        var buffer = NewBuffer();
        var canvas = new Canvas(buffer, Custom, 0, 0, 5, 5);

        canvas.Write(0, 2, "ABCDEFG");

        Assert.Equal("  ABC               ", RowText(buffer, 0));
    }

    [Fact]
    public void Canvas_clips_text_starting_left_of_its_edge()
    {
        var buffer = NewBuffer();
        var canvas = new Canvas(buffer, Custom, 0, 0, 5, 5);

        canvas.Write(0, -2, "ABCDE");

        Assert.Equal("CDE                 ", RowText(buffer, 0));
    }

    [Fact]
    public void Canvas_ignores_text_entirely_left_of_its_edge()
    {
        var buffer = NewBuffer();
        new Canvas(buffer, Custom, 0, 0, 5, 5).Write(0, -9, "AB");

        Assert.Equal(new string(' ', 20), RowText(buffer, 0));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(5)]
    public void Canvas_ignores_rows_outside_its_height(int row)
    {
        var buffer = NewBuffer();
        new Canvas(buffer, Custom, 0, 0, 20, 5).Write(row, 0, "X");

        for (var r = 0; r < 5; r++)
            Assert.Equal(new string(' ', 20), RowText(buffer, r));
    }

    [Fact]
    public void Canvas_ignores_a_start_column_beyond_its_width()
    {
        var buffer = NewBuffer();
        new Canvas(buffer, Custom, 0, 0, 5, 5).Write(0, 5, "X");

        Assert.Equal(new string(' ', 20), RowText(buffer, 0));
    }

    [Fact]
    public void Sub_canvas_offsets_and_clips_to_its_own_region()
    {
        var buffer = NewBuffer();
        var sub = new Canvas(buffer, Custom, 0, 0, 20, 5).Sub(top1: 1, left1: 3, width1: 4, height1: 2);

        sub.Write(0, 0, "ABCDEFGH");
        sub.Write(1, 0, "12");
        sub.Write(2, 0, "no");

        Assert.Equal("   ABCD             ", RowText(buffer, 1));
        Assert.Equal("   12               ", RowText(buffer, 2));
        Assert.Equal(new string(' ', 20), RowText(buffer, 3));
    }

    [Fact]
    public void WriteRow_pads_to_the_full_width_in_the_given_style()
    {
        var buffer = NewBuffer();
        var canvas = new Canvas(buffer, Custom, 0, 2, 10, 5);

        canvas.WriteRow(0, "hi", Style.Highlight);

        Assert.Equal(Custom.HighlightBackground, buffer[0, 2].Background);
        Assert.Equal(Custom.HighlightBackground, buffer[0, 11].Background);
        Assert.Equal(Custom.Background, buffer[0, 12].Background);
        Assert.Equal(Custom.Background, buffer[0, 1].Background);
    }

    // ---- ScreenBuffer ---------------------------------------------------------------------------

    [Fact]
    public void Buffer_starts_blank_in_the_given_colors()
    {
        var buffer = NewBuffer(3, 2);

        Assert.All(new[] { (0, 0), (1, 2) }, p =>
            Assert.Equal(new ScreenBuffer.Cell(' ', Custom.Text, Custom.Background), buffer[p.Item1, p.Item2]));
    }

    [Fact]
    public void Buffer_write_clips_outside_the_grid_without_throwing()
    {
        var buffer = NewBuffer(4, 2);

        buffer.Write(-1, 0, "X", ConsoleColor.Red, ConsoleColor.Black);
        buffer.Write(2, 0, "X", ConsoleColor.Red, ConsoleColor.Black);
        buffer.Write(0, -1, "ABC", ConsoleColor.Red, ConsoleColor.Black);
        buffer.Write(1, 2, "WXYZ", ConsoleColor.Red, ConsoleColor.Black);

        Assert.Equal("BC  ", RowText(buffer, 0));
        Assert.Equal("  WX", RowText(buffer, 1));
    }

    [Fact]
    public void First_flush_sends_every_cell()
    {
        var terminal = new FakeTerminal(11, 3); // usable width 10
        var buffer = NewBuffer(10, 3);
        buffer.Write(1, 0, "hello", ConsoleColor.Red, ConsoleColor.Black);

        buffer.Flush(terminal, previous: null);

        Assert.Equal("hello     ", terminal.Row(1)[..10]);
        Assert.Equal(ConsoleColor.Red, terminal.Cell(1, 0).Foreground);
        Assert.Equal(ConsoleColor.Yellow, terminal.Cell(0, 0).Foreground);
    }

    [Fact]
    public void Flush_after_an_identical_frame_writes_nothing()
    {
        var terminal = new FakeTerminal(11, 3);
        var first = NewBuffer(10, 3);
        var second = NewBuffer(10, 3);
        first.Write(0, 0, "same", ConsoleColor.Red, ConsoleColor.Black);
        second.Write(0, 0, "same", ConsoleColor.Red, ConsoleColor.Black);

        second.Flush(terminal, first);

        Assert.Empty(terminal.Writes);
    }

    [Fact]
    public void Flush_sends_only_the_changed_run()
    {
        var terminal = new FakeTerminal(11, 3);
        var first = NewBuffer(10, 3);
        var second = NewBuffer(10, 3);
        second.Write(1, 4, "XY", Custom.Text, Custom.Background);

        second.Flush(terminal, first);

        Assert.Equal([(1, 4, "XY")], terminal.Writes);
    }

    [Fact]
    public void Flush_splits_a_changed_run_where_the_colors_change()
    {
        var terminal = new FakeTerminal(11, 3);
        var first = NewBuffer(10, 3);
        var second = NewBuffer(10, 3);
        second.Write(0, 0, "AB", ConsoleColor.Red, ConsoleColor.Black);
        second.Write(0, 2, "CD", ConsoleColor.Green, ConsoleColor.Black);

        second.Flush(terminal, first);

        Assert.Equal([(0, 0, "AB"), (0, 2, "CD")], terminal.Writes);
    }

    [Fact]
    public void Flush_treats_a_color_only_change_as_a_change()
    {
        var terminal = new FakeTerminal(11, 3);
        var first = NewBuffer(10, 3);
        var second = NewBuffer(10, 3);
        first.Write(0, 0, "A", Custom.Text, Custom.Background);
        second.Write(0, 0, "A", Custom.HighlightText, Custom.HighlightBackground);

        second.Flush(terminal, first);

        Assert.Equal([(0, 0, "A")], terminal.Writes);
    }

    [Fact]
    public void Flush_against_a_previous_buffer_of_a_different_size_repaints_everything()
    {
        var terminal = new FakeTerminal(11, 3);
        var small = NewBuffer(5, 3);
        var big = NewBuffer(10, 3);

        big.Flush(terminal, small);

        Assert.Equal(3, terminal.Writes.Count); // one full-width run per row
        Assert.All(terminal.Writes, w => Assert.Equal(10, w.Text.Length));
    }
}
