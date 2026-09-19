namespace TMDEmulator.Tui;

internal readonly record struct Span(string Text, Role Role = Role.Normal);

/// <summary>
/// One logical line in the Main frame. Long lines wrap to the frame width.
/// <see cref="Fill"/> styles the remainder of the row, e.g. a full-width highlighted menu row.
/// </summary>
internal sealed record BodyLine(IReadOnlyList<Span> Spans, Role Fill = Role.Normal)
{
    public static BodyLine Empty { get; } = new(Array.Empty<Span>());

    public static BodyLine Of(string text, Role role = Role.Normal) => new(new[] { new Span(text, role) }, role);

    public string PlainText => string.Concat(Spans.Select(s => s.Text));
}
