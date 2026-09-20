using TMDEmulator.Terminal;
using TMDEmulator.Tests.Support;
using TMDEmulator.Ui;

namespace TMDEmulator.Tests;

public sealed class SelectionListAndLineEditorTests
{
    private static readonly Theme T = Theme.Default;

    private static (ScreenBuffer Buffer, Canvas Canvas) NewCanvas(int width = 20, int height = 6)
    {
        var buffer = new ScreenBuffer(width, height, T.Text, T.Background);
        return (buffer, new Canvas(buffer, T, 0, 0, width, height));
    }

    private static string RowText(ScreenBuffer b, int row) =>
        new(Enumerable.Range(0, b.Width).Select(c => b[row, c].Char).ToArray());

    // ---- SelectionList ------------------------------------------------------------------------

    [Fact]
    public void Selection_starts_on_the_first_entry()
    {
        Assert.Equal(0, new SelectionList(["a", "b", "c"]).Index);
    }

    [Fact]
    public void Down_and_up_move_the_selection_and_wrap_around()
    {
        var list = new SelectionList(["a", "b", "c"]);

        Assert.True(list.HandleKey(Press.Down));
        Assert.Equal(1, list.Index);
        list.HandleKey(Press.Down);
        list.HandleKey(Press.Down);
        Assert.Equal(0, list.Index); // wrapped past the end
        list.HandleKey(Press.Up);
        Assert.Equal(2, list.Index); // wrapped past the start
    }

    [Theory]
    [InlineData(ConsoleKey.Enter)]
    [InlineData(ConsoleKey.Escape)]
    [InlineData(ConsoleKey.LeftArrow)]
    [InlineData(ConsoleKey.A)]
    public void Other_keys_are_not_consumed(ConsoleKey key)
    {
        var list = new SelectionList(["a", "b"]);

        Assert.False(list.HandleKey(new ConsoleKeyInfo('\0', key, false, false, false)));
        Assert.Equal(0, list.Index);
    }

    [Fact]
    public void An_empty_list_ignores_navigation_instead_of_dividing_by_zero()
    {
        var list = new SelectionList([]);

        Assert.False(list.HandleKey(Press.Down));
        Assert.False(list.HandleKey(Press.Up));
    }

    [Fact]
    public void Reset_returns_to_the_first_entry()
    {
        var list = new SelectionList(["a", "b"]);
        list.HandleKey(Press.Down);

        list.Reset();

        Assert.Equal(0, list.Index);
    }

    [Fact]
    public void Render_highlights_only_the_selected_row_across_the_full_width()
    {
        var (buffer, canvas) = NewCanvas();
        var list = new SelectionList(["one", "two"]);
        list.HandleKey(Press.Down);

        list.Render(canvas, startRow: 1);

        Assert.StartsWith("  one", RowText(buffer, 1));
        Assert.StartsWith("  two", RowText(buffer, 2));
        Assert.Equal(T.Background, buffer[1, 0].Background);
        Assert.Equal(T.Text, buffer[1, 0].Foreground);
        for (var c = 0; c < 20; c++)
        {
            Assert.Equal(T.HighlightBackground, buffer[2, c].Background); // grey row
            Assert.Equal(T.HighlightText, buffer[2, c].Foreground);        // black text
        }
    }

    // ---- LineEditor ---------------------------------------------------------------------------

    private static LineEditor Typed(string initial, params ConsoleKeyInfo[] keys)
    {
        var editor = new LineEditor(initial);
        foreach (var key in keys)
            editor.HandleKey(key);
        return editor;
    }

    [Fact]
    public void Editor_starts_with_the_default_text_and_the_caret_at_the_end()
    {
        var editor = new LineEditor("/tmp/x");

        Assert.Equal("/tmp/x", editor.Text);
        Assert.Equal(6, editor.Caret);
    }

    [Fact]
    public void Typing_appends_at_the_caret()
    {
        var editor = Typed("ab", Press.Text("cd"));

        Assert.Equal("abcd", editor.Text);
        Assert.Equal(4, editor.Caret);
    }

    [Fact]
    public void Typing_in_the_middle_inserts()
    {
        var editor = Typed("ad", Press.Left, Press.Char('b'), Press.Char('c'));

        Assert.Equal("abcd", editor.Text);
        Assert.Equal(3, editor.Caret);
    }

    [Fact]
    public void Backspace_removes_the_character_before_the_caret()
    {
        var editor = Typed("abc", Press.Left, Press.Backspace);

        Assert.Equal("ac", editor.Text);
        Assert.Equal(1, editor.Caret);
    }

    [Fact]
    public void Backspace_at_the_start_does_nothing_but_is_consumed()
    {
        var editor = new LineEditor("abc");
        editor.HandleKey(Press.Home);

        Assert.True(editor.HandleKey(Press.Backspace));
        Assert.Equal("abc", editor.Text);
        Assert.Equal(0, editor.Caret);
    }

    [Fact]
    public void Delete_removes_the_character_under_the_caret()
    {
        var editor = Typed("abc", Press.Home, Press.Delete);

        Assert.Equal("bc", editor.Text);
        Assert.Equal(0, editor.Caret);
    }

    [Fact]
    public void Delete_at_the_end_does_nothing_but_is_consumed()
    {
        var editor = new LineEditor("abc");

        Assert.True(editor.HandleKey(Press.Delete));
        Assert.Equal("abc", editor.Text);
    }

    [Fact]
    public void Caret_moves_are_clamped_to_the_text()
    {
        var editor = Typed("ab", Press.Right, Press.Right);
        Assert.Equal(2, editor.Caret);

        editor = Typed("ab", Press.Left, Press.Left, Press.Left, Press.Left);
        Assert.Equal(0, editor.Caret);
    }

    [Fact]
    public void Home_and_end_jump_to_the_ends()
    {
        var editor = Typed("abc", Press.Home);
        Assert.Equal(0, editor.Caret);

        editor.HandleKey(Press.End);
        Assert.Equal(3, editor.Caret);
    }

    [Fact]
    public void Enter_escape_tab_and_ctrl_chars_are_not_consumed_or_inserted()
    {
        var editor = new LineEditor("abc");

        Assert.False(editor.HandleKey(Press.Enter));
        Assert.False(editor.HandleKey(Press.Escape));
        Assert.False(editor.HandleKey(Press.Tab));
        Assert.False(editor.HandleKey(Press.CtrlC));
        Assert.False(editor.HandleKey(Press.Up));
        Assert.Equal("abc", editor.Text);
    }

    [Fact]
    public void Any_printable_character_is_accepted_including_path_punctuation_and_unicode()
    {
        var editor = Typed("", Press.Text(@"C:\a b/é.txt"));

        Assert.Equal(@"C:\a b/é.txt", editor.Text);
    }

    [Fact]
    public void Render_shows_prompt_text_and_an_inverted_caret_cell()
    {
        var (buffer, canvas) = NewCanvas();
        var editor = Typed("abc", Press.Left); // caret on 'c'

        editor.Render(canvas, row: 2);

        Assert.StartsWith("> abc", RowText(buffer, 2));
        Assert.Equal(T.Background, buffer[2, 2].Background); // 'a' normal
        Assert.Equal('c', buffer[2, 4].Char);
        Assert.Equal(T.HighlightBackground, buffer[2, 4].Background); // caret cell is inverted
        Assert.Equal(T.HighlightText, buffer[2, 4].Foreground);
    }

    [Fact]
    public void Render_shows_the_caret_as_a_highlighted_blank_after_the_last_character()
    {
        var (buffer, canvas) = NewCanvas();

        new LineEditor("ab").Render(canvas, row: 0);

        Assert.Equal(' ', buffer[0, 4].Char);
        Assert.Equal(T.HighlightBackground, buffer[0, 4].Background);
    }

    [Fact]
    public void Long_text_scrolls_so_the_caret_and_tail_stay_visible()
    {
        var (buffer, canvas) = NewCanvas(width: 10); // prompt 2 => 8 cells for text
        var editor = new LineEditor("0123456789ABCDEF");

        editor.Render(canvas, row: 0);

        Assert.Equal("> 9ABCDEF ", RowText(buffer, 0)); // last 7 chars + caret cell
        Assert.Equal(T.HighlightBackground, buffer[0, 9].Background);
    }

    [Fact]
    public void Scrolled_text_shows_the_caret_when_moved_back_to_the_start()
    {
        var (buffer, canvas) = NewCanvas(width: 10);
        var editor = Typed("0123456789ABCDEF", Press.Home);

        editor.Render(canvas, row: 0);

        Assert.Equal("> 0123456", RowText(buffer, 0)[..9]);
        Assert.Equal(T.HighlightBackground, buffer[0, 2].Background);
    }
}
