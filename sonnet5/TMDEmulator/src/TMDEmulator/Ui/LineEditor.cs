namespace TMDEmulator.Ui;

/// <summary>A single-line text input with a caret: type, Backspace, Delete, Left/Right, Home/End.
/// Enter and Esc are left to the owning screen.</summary>
public sealed class LineEditor
{
    private const string Prompt = "> ";

    public LineEditor(string initialText)
    {
        Text = initialText;
        Caret = initialText.Length;
    }

    public string Text { get; private set; }
    public int Caret { get; private set; }

    /// <summary>Returns true if the key was an editing key and has been applied.</summary>
    public bool HandleKey(ConsoleKeyInfo key)
    {
        switch (key.Key)
        {
            case ConsoleKey.LeftArrow:
                Caret = Math.Max(0, Caret - 1);
                return true;
            case ConsoleKey.RightArrow:
                Caret = Math.Min(Text.Length, Caret + 1);
                return true;
            case ConsoleKey.Home:
                Caret = 0;
                return true;
            case ConsoleKey.End:
                Caret = Text.Length;
                return true;
            case ConsoleKey.Backspace:
                if (Caret > 0)
                {
                    Text = Text.Remove(Caret - 1, 1);
                    Caret--;
                }

                return true;
            case ConsoleKey.Delete:
                if (Caret < Text.Length)
                    Text = Text.Remove(Caret, 1);
                return true;
        }

        if (key.KeyChar != '\0' && !char.IsControl(key.KeyChar))
        {
            Text = Text.Insert(Caret, key.KeyChar.ToString());
            Caret++;
            return true;
        }

        return false;
    }

    /// <summary>Draw the prompt and text on one row. The caret is shown as an inverted cell and the
    /// text scrolls horizontally so the caret is always visible.</summary>
    public void Render(Canvas canvas, int row)
    {
        var available = Math.Max(1, canvas.Width - Prompt.Length);
        var start = Math.Max(0, Caret - (available - 1));
        var visible = Text.Length - start > available ? Text.Substring(start, available) : Text[start..];

        canvas.Write(row, 0, Prompt);
        canvas.Write(row, Prompt.Length, visible);

        var caretColumn = Prompt.Length + (Caret - start);
        var under = Caret < Text.Length ? Text[Caret].ToString() : " ";
        canvas.Write(row, caretColumn, under, Style.Highlight);
    }
}
