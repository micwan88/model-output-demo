namespace TMDEmulator.Tui;

internal enum ConfirmOutcome
{
    Pending,
    Yes,
    No,
}

/// <summary>Yes/No choice navigated with the arrow keys. Defaults to No; Esc means No.</summary>
internal sealed class ConfirmPrompt
{
    public bool YesSelected { get; private set; }

    public ConfirmOutcome HandleKey(ConsoleKeyInfo key)
    {
        switch (key.Key)
        {
            case ConsoleKey.LeftArrow:
            case ConsoleKey.RightArrow:
            case ConsoleKey.UpArrow:
            case ConsoleKey.DownArrow:
            case ConsoleKey.Tab:
                YesSelected = !YesSelected;
                return ConfirmOutcome.Pending;
            case ConsoleKey.Enter:
                return YesSelected ? ConfirmOutcome.Yes : ConfirmOutcome.No;
            case ConsoleKey.Escape:
                return ConfirmOutcome.No;
            default:
                return ConfirmOutcome.Pending;
        }
    }

    public BodyLine Render() => new(new[]
    {
        new Span("  "),
        new Span("  Yes  ", YesSelected ? Role.Highlight : Role.Normal),
        new Span("  "),
        new Span("  No  ", YesSelected ? Role.Normal : Role.Highlight),
    });
}
