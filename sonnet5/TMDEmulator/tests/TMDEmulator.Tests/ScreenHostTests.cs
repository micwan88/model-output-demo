using TMDEmulator.Terminal;
using TMDEmulator.Tests.Support;
using TMDEmulator.Ui;

namespace TMDEmulator.Tests;

public sealed class ScreenHostTests : IDisposable
{
    private readonly Harness _h = new();

    public void Dispose() => _h.Dispose();

    private FakeTerminal T => _h.Terminal;

    private static readonly Theme Default = Theme.Default;

    // ---- startup + layout ----------------------------------------------------------------------

    [Fact]
    public void Startup_shows_the_first_level_menu()
    {
        Assert.Equal("Main Menu", _h.Host.Current.Title);
        Assert.Equal(1, _h.Host.Depth);
        Assert.True(T.Shows("Remote MZMK Setup"));
        Assert.True(T.Shows("Key Management"));
        Assert.True(T.Shows("Exit"));
    }

    [Fact]
    public void Startup_menu_lists_only_the_first_level_items_in_story_order()
    {
        var rows = new[] { T.RowOf("Remote MZMK Setup"), T.RowOf("Key Management"), T.RowOf("Exit") };

        Assert.Equal(rows.OrderBy(r => r), rows);
        Assert.False(T.Shows("Export MZMK Initialization Key"));
        Assert.False(T.Shows("View Keys"));
    }

    [Fact]
    public void Screen_has_two_frames_main_on_top_and_status_at_the_bottom()
    {
        var statusTop = T.Height - 5;

        Assert.StartsWith("┌─ TMD Emulator ─", T.Row(0));
        Assert.StartsWith("└", T.Row(statusTop - 1)); // main frame closes
        Assert.StartsWith("┌", T.Row(statusTop));       // status frame opens
        Assert.StartsWith("└", T.Row(T.Height - 1));    // status frame closes on the last row
        Assert.Equal('┐', T.Cell(0, T.Width - 2).Char);
        Assert.Equal('┘', T.Cell(T.Height - 1, T.Width - 2).Char);
    }

    [Fact]
    public void The_last_terminal_column_is_left_unused_to_avoid_console_scrolling()
    {
        for (var r = 0; r < T.Height; r++)
            Assert.Equal(' ', T.Cell(r, T.Width - 1).Char);
    }

    [Fact]
    public void Status_bar_shows_the_navigation_legend_under_its_top_border()
    {
        Assert.Contains("↑/↓ Navigate", T.Row(_h.LegendRow));
        Assert.Contains("Enter Select", T.Row(_h.LegendRow));
        Assert.Contains("Esc Back", T.Row(_h.LegendRow));
    }

    [Fact]
    public void Breadcrumb_starts_with_the_main_menu()
    {
        Assert.Contains("Main Menu", T.Row(1));
    }

    [Fact]
    public void Default_colors_black_background_white_text_grey_highlight_with_black_text()
    {
        var highlighted = T.RowOf("Remote MZMK Setup");
        var plain = T.RowOf("Key Management");

        Assert.Equal(ConsoleColor.Black, T.Cell(plain, 3).Background);
        Assert.Equal(ConsoleColor.White, T.Cell(plain, 5).Foreground);
        Assert.Equal(ConsoleColor.Gray, T.Cell(highlighted, 5).Background);
        Assert.Equal(ConsoleColor.Black, T.Cell(highlighted, 5).Foreground);
        Assert.Equal(ConsoleColor.Black, T.Cell(2, 10).Background); // empty area
    }

    [Fact]
    public void A_configured_theme_changes_the_colors_that_are_drawn()
    {
        using var h = new Harness(theme: new Theme(
            ConsoleColor.DarkBlue, ConsoleColor.Yellow, ConsoleColor.Cyan, ConsoleColor.DarkRed, ConsoleColor.Magenta));
        var highlighted = h.Terminal.RowOf("Remote MZMK Setup");
        var plain = h.Terminal.RowOf("Key Management");

        Assert.Equal(ConsoleColor.DarkBlue, h.Terminal.Cell(plain, 5).Background);
        Assert.Equal(ConsoleColor.Yellow, h.Terminal.Cell(plain, 5).Foreground);
        Assert.Equal(ConsoleColor.Cyan, h.Terminal.Cell(highlighted, 5).Background);
        Assert.Equal(ConsoleColor.DarkRed, h.Terminal.Cell(highlighted, 5).Foreground);
    }

    [Fact]
    public void Highlight_covers_the_whole_row_width_of_the_content_area()
    {
        var row = T.RowOf("Remote MZMK Setup");

        for (var c = 2; c < T.Width - 3; c++)
            Assert.Equal(ConsoleColor.Gray, T.Cell(row, c).Background);
        Assert.Equal(ConsoleColor.Black, T.Cell(row, 1).Background); // border padding stays black
    }

    // ---- navigation ------------------------------------------------------------------------------

    private string HighlightedText()
    {
        for (var r = 0; r < T.Height; r++)
            if (T.Cell(r, 5).Background == ConsoleColor.Gray && T.Cell(r, 5).Char != ' ')
                return T.Row(r).Trim();
        return "";
    }

    [Fact]
    public void Down_and_up_move_the_highlight_and_wrap()
    {
        Assert.Contains("Remote MZMK Setup", HighlightedText());

        _h.Send(Press.Down);
        Assert.Contains("Key Management", HighlightedText());

        _h.Send(Press.Down, Press.Down);
        Assert.Contains("Remote MZMK Setup", HighlightedText()); // wrapped from Exit

        _h.Send(Press.Up);
        Assert.Contains("Exit", HighlightedText()); // wrapped from the top
    }

    [Fact]
    public void Enter_on_remote_mzmk_setup_opens_the_submenu_with_its_four_options()
    {
        _h.Send(Press.Enter);

        Assert.Equal("Remote MZMK Setup", _h.Host.Current.Title);
        Assert.Contains("Main Menu › Remote MZMK Setup", T.Row(1));
        var rows = new[]
        {
            T.RowOf("Export MZMK Initialization Key"), T.RowOf("Finalize Remote MZMK"),
            T.RowOf("Uninstalling a MZMK"), T.RowOf("Back"),
        };
        Assert.Equal(rows.OrderBy(r => r), rows);
        Assert.Equal(4, rows.Distinct().Count());
    }

    [Fact]
    public void Esc_goes_back_one_screen()
    {
        _h.Send(Press.Enter, Press.Escape);

        Assert.Equal("Main Menu", _h.Host.Current.Title);
        Assert.True(T.Shows("Key Management"));
    }

    [Fact]
    public void The_back_item_goes_back_like_esc()
    {
        _h.Send(Press.Enter, Press.Up, Press.Enter); // Up wraps to "Back", Enter selects it

        Assert.Equal("Main Menu", _h.Host.Current.Title);
    }

    [Fact]
    public void Esc_on_the_top_level_menu_does_nothing()
    {
        _h.Send(Press.Escape);

        Assert.Equal(1, _h.Host.Depth);
        Assert.False(_h.Host.ExitRequested);
        Assert.True(T.Shows("Remote MZMK Setup"));
    }

    [Fact]
    public void Pop_on_the_top_level_screen_is_a_no_op()
    {
        _h.Host.Pop();

        Assert.Equal(1, _h.Host.Depth);
    }

    [Fact]
    public void Key_management_has_view_keys_and_back()
    {
        _h.Send(Press.Down, Press.Enter);

        Assert.Equal("Key Management", _h.Host.Current.Title);
        Assert.True(T.RowOf("View Keys") < T.RowOf("Back"));
        Assert.False(T.Shows("Export MZMK"));
    }

    [Fact]
    public void Key_management_back_item_returns_to_the_main_menu()
    {
        _h.Send(Press.Down, Press.Enter, Press.Down, Press.Enter); // Key Management -> Back

        Assert.Equal("Main Menu", _h.Host.Current.Title);
        Assert.Equal(1, _h.Host.Depth);
    }

    [Theory]
    [InlineData(new[] { "Down", "Enter", "Enter" }, "View Keys")]
    [InlineData(new[] { "Enter", "Down", "Enter" }, "Finalize Remote MZMK")]
    [InlineData(new[] { "Enter", "Down", "Down", "Enter" }, "Uninstalling a MZMK")]
    public void Deferred_functions_show_a_placeholder_and_esc_returns(string[] path, string title)
    {
        _h.Send(path.Select(p => p == "Down" ? Press.Down : Press.Enter).ToArray());

        Assert.Equal(title, _h.Host.Current.Title);
        Assert.Contains(title, T.Row(1));
        Assert.True(T.Shows("Not implemented yet"));
        Assert.Contains("Esc Back", T.Row(_h.LegendRow));

        _h.Send(Press.Enter, Press.Down, Press.Up); // no effect on a placeholder
        Assert.Equal(title, _h.Host.Current.Title);

        _h.Send(Press.Escape);
        Assert.NotEqual(title, _h.Host.Current.Title);
    }

    // ---- exit + terminal lifecycle -----------------------------------------------------------------

    [Fact]
    public void Exit_item_ends_the_run_loop_and_restores_the_terminal()
    {
        T.Enqueue(Press.Down, Press.Down, Press.Enter); // Exit

        _h.Host.Run();

        Assert.True(_h.Host.ExitRequested);
        Assert.True(T.Initialized);
        Assert.True(T.Restored);
    }

    [Fact]
    public void Ctrl_c_exits_from_any_screen()
    {
        _h.Send(Press.Enter, Press.Enter); // deep in Export screen

        _h.Send(Press.CtrlC);

        Assert.True(_h.Host.ExitRequested);
    }

    [Fact]
    public void Ctrl_c_in_the_run_loop_exits_cleanly()
    {
        T.Enqueue(Press.CtrlC);

        _h.Host.Run();

        Assert.True(T.Restored);
    }

    private sealed class ThrowingScreen : IScreen
    {
        public string Title => "Boom";
        public string Legend => "";
        public void Render(Canvas canvas) { }
        public void HandleKey(ConsoleKeyInfo key, ScreenHost host) => throw new InvalidOperationException("boom");
    }

    [Fact]
    public void The_terminal_is_restored_even_if_a_screen_throws()
    {
        var terminal = new FakeTerminal();
        var host = new ScreenHost(terminal, Default, new ThrowingScreen(), []);
        terminal.Enqueue(Press.Enter);

        Assert.Throws<InvalidOperationException>(host.Run);

        Assert.True(terminal.Restored);
    }

    // ---- redraw + resize -----------------------------------------------------------------------------

    [Fact]
    public void Redrawing_an_unchanged_screen_writes_nothing()
    {
        var before = T.Writes.Count;

        _h.Host.Redraw();

        Assert.Equal(before, T.Writes.Count);
    }

    [Fact]
    public void Moving_the_highlight_rewrites_only_the_two_affected_rows()
    {
        var before = T.Writes.Count;

        _h.Send(Press.Down);

        var rewritten = T.Writes.Skip(before).Select(w => w.Row).Distinct().OrderBy(r => r).ToArray();
        Assert.Equal([T.RowOf("Remote MZMK Setup"), T.RowOf("Key Management")], rewritten);
    }

    [Fact]
    public void Waiting_without_a_size_change_does_not_redraw()
    {
        var writesAtWait = -1;
        T.EnqueueAction(() => writesAtWait = T.Writes.Count);
        T.Enqueue(Press.CtrlC);

        _h.Host.Run();

        Assert.Equal(writesAtWait, T.Writes.Count);
    }

    [Fact]
    public void Resizing_the_window_repaints_the_layout_at_the_new_size()
    {
        var clearsBefore = T.ClearCount;
        T.EnqueueAction(() => T.Resize(100, 40));
        T.Enqueue(Press.CtrlC);

        _h.Host.Run();

        Assert.True(T.ClearCount > clearsBefore);
        Assert.StartsWith("┌─ TMD Emulator", T.Row(0));
        Assert.Equal('┐', T.Cell(0, 98).Char);
        Assert.StartsWith("└", T.Row(39));
        Assert.True(T.Shows("Remote MZMK Setup"));
    }

    // ---- too small ---------------------------------------------------------------------------------------

    [Theory]
    [InlineData(59, 30)]
    [InlineData(80, 21)]
    public void A_terminal_below_the_minimum_size_shows_a_message_instead_of_the_layout(int width, int height)
    {
        using var h = new Harness(width, height);

        Assert.True(h.Terminal.Shows("Terminal too small"));
        Assert.Contains($"{ScreenHost.MinWidth}x{ScreenHost.MinHeight}", h.Terminal.Row(0));
        Assert.False(h.Terminal.Shows("Remote MZMK Setup"));
        Assert.Equal(ConsoleColor.Red, h.Terminal.Cell(0, 0).Foreground);
    }

    [Fact]
    public void A_tiny_terminal_gets_a_clipped_message_without_crashing()
    {
        using var h = new Harness(20, 5);

        Assert.True(h.Terminal.Shows("Terminal too small"));
        Assert.Equal('…', h.Terminal.Cell(0, 18).Char); // cut to the 19 usable columns
    }

    [Fact]
    public void Keys_are_ignored_while_the_terminal_is_too_small_but_ctrl_c_still_quits()
    {
        using var h = new Harness(50, 15);

        h.Send(Press.Enter, Press.Down, Press.Escape);
        Assert.Equal(1, h.Host.Depth);
        Assert.Equal("Main Menu", h.Host.Current.Title);

        h.Send(Press.CtrlC);
        Assert.True(h.Host.ExitRequested);
    }

    [Fact]
    public void Enlarging_a_too_small_terminal_brings_the_menu_back()
    {
        using var h = new Harness(50, 15);

        h.Terminal.Resize(80, 30);
        h.Host.Redraw();

        Assert.True(h.Terminal.Shows("Remote MZMK Setup"));
        Assert.False(h.Terminal.Shows("Terminal too small"));
    }

    [Fact]
    public void The_minimum_size_itself_is_usable_and_never_writes_past_the_edge()
    {
        using var h = new Harness(ScreenHost.MinWidth, ScreenHost.MinHeight);

        h.OpenExportScreen(); // the busiest screen; FakeTerminal throws on any out-of-bounds write

        Assert.True(h.Terminal.Shows("Output folder for the MZMK Initialization file"));
        Assert.True(h.Terminal.Shows("> "));
    }

    // ---- status messages -----------------------------------------------------------------------------------

    [Fact]
    public void Startup_errors_are_shown_in_red_under_the_legend_until_the_first_key()
    {
        using var h = new Harness(startupErrors: ["tmdemulator.json: invalid color for 'error'"]);

        var row = h.MessageRow;
        Assert.Contains("invalid color for 'error'", h.Terminal.Row(row));
        Assert.Equal(ConsoleColor.Red, h.Terminal.Cell(row, 2).Foreground);
        Assert.True(row > h.LegendRow);

        h.Send(Press.Down);
        Assert.DoesNotContain("invalid color", h.Terminal.Row(row));
    }

    [Fact]
    public void Several_startup_errors_are_joined_into_the_message()
    {
        using var h = new Harness(startupErrors: ["first problem", "second problem"]);

        Assert.Contains("first problem; second problem", h.Terminal.Row(h.MessageRow));
    }

    [Fact]
    public void Info_messages_are_not_red()
    {
        _h.Host.ShowInfo("all good");
        _h.Host.Redraw();

        Assert.Contains("all good", T.Row(_h.MessageRow));
        Assert.Equal(ConsoleColor.White, T.Cell(_h.MessageRow, 2).Foreground);
    }

    [Fact]
    public void A_long_message_wraps_onto_a_second_line()
    {
        const int innerWidth = 75; // 80 columns => 79 usable, minus borders and padding
        var message = new string('a', innerWidth) + "END";
        _h.Host.ShowError(message);
        _h.Host.Redraw();

        Assert.Equal(new string('a', innerWidth), T.Row(_h.MessageRow).Substring(2, innerWidth));
        Assert.Contains("END", T.Row(_h.MessageRow + 1));
        Assert.Equal(ConsoleColor.Red, T.Cell(_h.MessageRow + 1, 2).Foreground);
    }

    [Fact]
    public void A_message_longer_than_two_lines_ends_with_an_ellipsis_inside_the_frame()
    {
        _h.Host.ShowError(new string('x', 400));
        _h.Host.Redraw();

        var last = T.Row(_h.MessageRow + 1);
        Assert.Contains("…", last);
        Assert.Equal('│', T.Cell(_h.MessageRow + 1, T.Width - 2).Char); // right border intact
    }

    [Fact]
    public void An_error_is_cleared_by_the_next_key_press()
    {
        _h.Host.ShowError("something failed");
        _h.Host.Redraw();
        Assert.True(T.Shows("something failed"));

        _h.Send(Press.Down);

        Assert.False(T.Shows("something failed"));
    }
}
