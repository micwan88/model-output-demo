namespace TMDEmulator.Tui;

internal enum InputOutcome
{
    Editing,
    Submitted,
    Cancelled,
}

/// <summary>Single-line editable text field with a visible caret.</summary>
internal sealed class TextInput
{
    public TextInput(string initialValue)
    {
        Value = initialValue;
        Caret = initialValue.Length;
    }

    public string Value { get; private set; }

    /// <summary>Caret position as a UTF-16 index into <see cref="Value"/>.</summary>
    public int Caret { get; private set; }

    public InputOutcome HandleKey(ConsoleKeyInfo key)
    {
        switch (key.Key)
        {
            case ConsoleKey.Enter:
                return InputOutcome.Submitted;
            case ConsoleKey.Escape:
                return InputOutcome.Cancelled;
            case ConsoleKey.LeftArrow:
                Caret = PreviousBoundary(Caret);
                break;
            case ConsoleKey.RightArrow:
                Caret = NextBoundary(Caret);
                break;
            case ConsoleKey.Home:
                Caret = 0;
                break;
            case ConsoleKey.End:
                Caret = Value.Length;
                break;
            case ConsoleKey.Backspace:
                if (Caret > 0)
                {
                    int start = PreviousBoundary(Caret);
                    Value = Value.Remove(start, Caret - start);
                    Caret = start;
                }

                break;
            case ConsoleKey.Delete:
                if (Caret < Value.Length)
                {
                    Value = Value.Remove(Caret, NextBoundary(Caret) - Caret);
                }

                break;
            default:
                if (!char.IsControl(key.KeyChar))
                {
                    Value = Value.Insert(Caret, key.KeyChar.ToString());
                    Caret++;
                }

                break;
        }

        return InputOutcome.Editing;
    }

    /// <summary>Renders "&gt; value" with the character under the caret highlighted.</summary>
    public BodyLine Render()
    {
        int caretEnd = Caret < Value.Length ? NextBoundary(Caret) : Caret;
        string underCaret = Caret < Value.Length ? Value[Caret..caretEnd] : " ";
        return new BodyLine(new[]
        {
            new Span("> "),
            new Span(Value[..Caret]),
            new Span(underCaret, Role.Highlight),
            new Span(Value[caretEnd..]),
        });
    }

    private int PreviousBoundary(int index) =>
        index >= 2 && char.IsSurrogatePair(Value[index - 2], Value[index - 1]) ? index - 2 : Math.Max(index - 1, 0);

    private int NextBoundary(int index) =>
        index + 1 < Value.Length && char.IsSurrogatePair(Value[index], Value[index + 1]) ? index + 2 : Math.Min(index + 1, Value.Length);
}
