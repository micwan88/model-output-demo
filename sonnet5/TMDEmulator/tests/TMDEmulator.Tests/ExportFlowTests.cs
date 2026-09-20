using System.Text;
using TMDEmulator.Tests.Support;
using TMDEmulator.Ui;

namespace TMDEmulator.Tests;

/// <summary>Drives the Export MZMK Initialization Key function through the real menus and screen host.</summary>
public sealed class ExportFlowTests : IDisposable
{
    private readonly Harness _h = new();

    public void Dispose() => _h.Dispose();

    private FakeTerminal T => _h.Terminal;

    /// <summary>Delete the pre-filled default folder text so a new path can be typed.</summary>
    private Harness ClearInput() => _h.Send(Enumerable.Repeat(Press.Backspace, 300).ToArray());

    private void AssertRedMessageContains(string text)
    {
        var row = T.RowOf(text);
        Assert.True(row >= _h.MessageRow, $"'{text}' must be under the legend (row {_h.LegendRow}), was on row {row}");
        var col = T.Row(row).IndexOf(text, StringComparison.Ordinal);
        Assert.Equal(ConsoleColor.Red, T.Cell(row, col).Foreground);
    }

    // ---- what the screen shows -----------------------------------------------------------------------

    [Fact]
    public void Screen_shows_curve_private_key_and_public_key()
    {
        _h.OpenExportScreen();

        Assert.Equal("Export MZMK Initialization Key", _h.Host.Current.Title);
        Assert.Contains("Main Menu › Remote MZMK Setup › Export MZMK Initialization Key", T.Row(1));
        Assert.True(T.Shows("Curve type: secp256r1"));
        Assert.True(T.Shows(ReferenceKey.CurveOid));
        Assert.True(T.Shows("Private key (HEX scalar)"));
        Assert.True(T.Shows("Public key (HEX"));
        Assert.Contains(ReferenceKey.PrivateScalarHex, T.Squashed());
        Assert.Contains(ReferenceKey.PublicPointHex, T.Squashed());
    }

    [Fact]
    public void Long_hex_values_wrap_inside_the_frame()
    {
        _h.OpenExportScreen();

        var first = T.RowOf("Public key (HEX") + 1;
        Assert.StartsWith("04A43E23", T.Row(first).Substring(4).TrimStart());
        Assert.Equal('│', T.Cell(first, T.Width - 2).Char);
    }

    [Fact]
    public void The_path_prompt_is_prefilled_with_the_current_absolute_directory_and_editable()
    {
        _h.OpenExportScreen();

        Assert.True(T.Shows("> " + _h.Dir.Path));
        Assert.True(Path.IsPathRooted(_h.Dir.Path));
        Assert.Contains("←/→ Move", T.Row(_h.LegendRow));
        Assert.Contains("Enter Save", T.Row(_h.LegendRow));
        Assert.Contains("Esc Back", T.Row(_h.LegendRow));
    }

    [Fact]
    public void The_caret_cell_is_highlighted_at_the_end_of_the_path()
    {
        _h.OpenExportScreen();

        var row = T.RowOf("> " + _h.Dir.Path);
        var caretCol = 4 + _h.Dir.Path.Length; // content starts at col 2, prompt "> " is 2 wide
        Assert.Equal(ConsoleColor.Gray, T.Cell(row, caretCol).Background);
    }

    [Fact]
    public void Esc_leaves_without_writing_anything()
    {
        _h.OpenExportScreen();

        _h.Send(Press.Escape);

        Assert.Equal("Remote MZMK Setup", _h.Host.Current.Title);
        Assert.Empty(Directory.GetFileSystemEntries(_h.Dir.Path));
    }

    // ---- saving ----------------------------------------------------------------------------------------

    [Fact]
    public void Enter_saves_the_file_in_the_default_folder_and_returns_to_the_menu()
    {
        _h.OpenExportScreen();

        _h.Send(Press.Enter);

        var path = _h.ExpectedFilePath;
        Assert.True(File.Exists(path));
        Assert.Equal(Encoding.ASCII.GetBytes(ReferenceKey.PublicPointHex), File.ReadAllBytes(path));
        Assert.Equal("Remote MZMK Setup", _h.Host.Current.Title); // back to menu
        Assert.Single(Directory.GetFiles(_h.Dir.Path));
        Assert.Equal(Harness.ExpectedFileName, Path.GetFileName(path));
    }

    [Fact]
    public void After_saving_the_full_path_is_confirmed_in_the_status_bar_and_not_in_red()
    {
        _h.OpenExportScreen();

        _h.Send(Press.Enter);

        var row = T.RowOf("Saved: ");
        Assert.True(row >= _h.MessageRow);
        Assert.Contains(Harness.ExpectedFileName, T.Squashed());
        Assert.Equal(ConsoleColor.White, T.Cell(row, 2).Foreground);
    }

    [Fact]
    public void The_saved_message_disappears_on_the_next_key()
    {
        _h.OpenExportScreen();
        _h.Send(Press.Enter);

        _h.Send(Press.Down);

        Assert.False(T.Shows("Saved: "));
    }

    [Fact]
    public void A_typed_folder_replaces_the_default()
    {
        var target = Directory.CreateDirectory(_h.Dir.Combine("out")).FullName;
        _h.OpenExportScreen();
        ClearInput().Send(Press.Text(target)).Send(Press.Enter);

        Assert.True(File.Exists(Path.Combine(target, Harness.ExpectedFileName)));
        Assert.False(File.Exists(_h.ExpectedFilePath));
    }

    [Fact]
    public void Editing_the_middle_of_the_default_path_works()
    {
        var sub = Directory.CreateDirectory(_h.Dir.Combine("sub")).FullName;
        _h.OpenExportScreen();

        _h.Send(Press.Char('/')).Send(Press.Text("sub")); // append "/sub" to the default path
        _h.Send(Press.Enter);

        Assert.True(File.Exists(Path.Combine(sub, Harness.ExpectedFileName)));
    }

    // ---- validation: errors in red, under the legend ------------------------------------------------------

    [Fact]
    public void An_empty_path_is_rejected_in_red_and_the_prompt_stays_open()
    {
        _h.OpenExportScreen();
        ClearInput().Send(Press.Enter);

        AssertRedMessageContains("Output folder must not be empty");
        Assert.Equal("Export MZMK Initialization Key", _h.Host.Current.Title);
    }

    [Fact]
    public void A_folder_that_does_not_exist_is_rejected_in_red_and_not_created()
    {
        var missing = _h.Dir.Combine("no-such-folder");
        _h.OpenExportScreen();
        ClearInput().Send(Press.Text(missing)).Send(Press.Enter);

        AssertRedMessageContains("Folder does not exist");
        Assert.Contains(Path.GetFileName(missing), T.Squashed());
        Assert.False(Directory.Exists(missing));
        Assert.Equal("Export MZMK Initialization Key", _h.Host.Current.Title);
    }

    [Fact]
    public void The_error_goes_away_on_the_next_key_and_the_user_can_correct_the_path_and_retry()
    {
        _h.OpenExportScreen();
        ClearInput().Send(Press.Enter);
        Assert.True(T.Shows("must not be empty"));

        _h.Send(Press.Text(_h.Dir.Path));
        Assert.False(T.Shows("must not be empty"));
        _h.Send(Press.Enter);

        Assert.True(File.Exists(_h.ExpectedFilePath));
    }

    // ---- overwrite ---------------------------------------------------------------------------------------------

    private void PrepareExistingFile() => File.WriteAllText(_h.ExpectedFilePath, "OLD CONTENT");

    [Fact]
    public void An_existing_file_triggers_an_overwrite_question_with_yes_highlighted_first()
    {
        PrepareExistingFile();
        _h.OpenExportScreen();

        _h.Send(Press.Enter);

        Assert.True(T.Shows("File already exists"));
        Assert.True(T.Shows("Overwrite it?"));
        var yes = T.RowOf("Yes");
        var no = T.RowOf("No");
        Assert.True(yes < no);
        Assert.Equal(ConsoleColor.Gray, T.Cell(yes, 5).Background);
        Assert.Equal(ConsoleColor.Black, T.Cell(no, 5).Background);
        Assert.Contains("Esc No", T.Row(_h.LegendRow));
        Assert.Equal("OLD CONTENT", File.ReadAllText(_h.ExpectedFilePath)); // nothing written yet
    }

    [Fact]
    public void Answering_yes_overwrites_and_returns_to_the_menu()
    {
        PrepareExistingFile();
        _h.OpenExportScreen();

        _h.Send(Press.Enter, Press.Enter); // save -> "Yes"

        Assert.Equal(ReferenceKey.PublicPointHex, File.ReadAllText(_h.ExpectedFilePath));
        Assert.Equal("Remote MZMK Setup", _h.Host.Current.Title);
        Assert.True(T.Shows("Saved: "));
    }

    [Fact]
    public void Answering_no_keeps_the_old_file_and_goes_back_to_the_path_prompt()
    {
        PrepareExistingFile();
        _h.OpenExportScreen();

        _h.Send(Press.Enter, Press.Down, Press.Enter); // save -> "No"

        Assert.Equal("OLD CONTENT", File.ReadAllText(_h.ExpectedFilePath));
        Assert.Equal("Export MZMK Initialization Key", _h.Host.Current.Title);
        Assert.True(T.Shows("> " + _h.Dir.Path));
        Assert.False(T.Shows("Overwrite it?"));
    }

    [Fact]
    public void Esc_on_the_overwrite_question_means_no()
    {
        PrepareExistingFile();
        _h.OpenExportScreen();

        _h.Send(Press.Enter, Press.Escape);

        Assert.Equal("OLD CONTENT", File.ReadAllText(_h.ExpectedFilePath));
        Assert.Equal("Export MZMK Initialization Key", _h.Host.Current.Title);
        Assert.False(T.Shows("Overwrite it?"));
        Assert.Contains("Enter Save", T.Row(_h.LegendRow));
    }

    [Fact]
    public void After_no_the_user_can_pick_another_folder_and_save_there()
    {
        PrepareExistingFile();
        var other = Directory.CreateDirectory(_h.Dir.Combine("other")).FullName;
        _h.OpenExportScreen();
        _h.Send(Press.Enter, Press.Escape);

        ClearInput().Send(Press.Text(other)).Send(Press.Enter);

        Assert.Equal("OLD CONTENT", File.ReadAllText(_h.ExpectedFilePath));
        Assert.True(File.Exists(Path.Combine(other, Harness.ExpectedFileName)));
    }

    [Fact]
    public void The_overwrite_question_always_starts_on_yes_when_asked_again()
    {
        PrepareExistingFile();
        _h.OpenExportScreen();
        _h.Send(Press.Enter, Press.Down, Press.Enter); // ask, choose No

        _h.Send(Press.Enter); // ask again

        Assert.Equal(ConsoleColor.Gray, T.Cell(T.RowOf("Yes"), 5).Background);
    }

    [Fact]
    public void Unrelated_keys_are_ignored_while_the_overwrite_question_is_open()
    {
        PrepareExistingFile();
        _h.OpenExportScreen();
        _h.Send(Press.Enter); // question shown

        _h.Send(Press.Char('x'), Press.Tab, Press.Left, Press.Backspace);

        Assert.True(T.Shows("Overwrite it?"));
        Assert.Equal(ConsoleColor.Gray, T.Cell(T.RowOf("Yes"), 5).Background);
        Assert.Equal("OLD CONTENT", File.ReadAllText(_h.ExpectedFilePath));
        _h.Send(Press.Escape);
        Assert.True(T.Shows("> " + _h.Dir.Path)); // the typed 'x' did not leak into the path
    }

    [Fact]
    public void The_overwrite_question_wraps_its_selection()
    {
        PrepareExistingFile();
        _h.OpenExportScreen();
        _h.Send(Press.Enter, Press.Up); // wraps from Yes to No

        Assert.Equal(ConsoleColor.Gray, T.Cell(T.RowOf("No"), 5).Background);
    }

    // ---- write failure ----------------------------------------------------------------------------------------------

    [Fact]
    public void A_write_failure_is_shown_in_red_and_the_prompt_stays_open()
    {
        // A directory squatting on the target file name makes File.Exists false but the write impossible.
        Directory.CreateDirectory(_h.ExpectedFilePath);
        _h.OpenExportScreen();

        _h.Send(Press.Enter);

        AssertRedMessageContains("Cannot write");
        Assert.Equal("Export MZMK Initialization Key", _h.Host.Current.Title);
        Assert.True(Directory.Exists(_h.ExpectedFilePath)); // untouched
    }

    [Fact]
    public void A_write_failure_after_answering_yes_is_also_reported()
    {
        PrepareExistingFile();
        _h.OpenExportScreen();
        _h.Send(Press.Enter); // overwrite question shown
        File.Delete(_h.ExpectedFilePath);
        Directory.CreateDirectory(_h.ExpectedFilePath); // now impossible to overwrite

        _h.Send(Press.Enter); // Yes

        AssertRedMessageContains("Cannot write");
        Assert.Equal("Export MZMK Initialization Key", _h.Host.Current.Title);
    }

    // ---- direct screen API -------------------------------------------------------------------------------------------

    [Fact]
    public void Screen_reports_when_it_is_waiting_for_an_overwrite_answer()
    {
        PrepareExistingFile();
        _h.OpenExportScreen();
        var screen = Assert.IsType<ExportInitKeyScreen>(_h.Host.Current);
        Assert.False(screen.AwaitingOverwriteAnswer);

        _h.Send(Press.Enter);

        Assert.True(screen.AwaitingOverwriteAnswer);
    }
}
