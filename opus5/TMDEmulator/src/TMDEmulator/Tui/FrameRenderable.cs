using Spectre.Console;
using Spectre.Console.Rendering;

namespace TMDEmulator.Tui;

/// <summary>
/// Renders composed rows as one Spectre.Console write: each row is positioned with a
/// cursor control sequence and styled from the theme, so a frame is emitted in a single call.
/// </summary>
internal sealed class FrameRenderable : IRenderable
{
    private readonly IReadOnlyList<IReadOnlyList<Span>> _rows;
    private readonly Theme _theme;

    public FrameRenderable(IReadOnlyList<IReadOnlyList<Span>> rows, Theme theme)
    {
        _rows = rows;
        _theme = theme;
    }

    public Measurement Measure(RenderOptions options, int maxWidth) => new(maxWidth, maxWidth);

    public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
    {
        for (int i = 0; i < _rows.Count; i++)
        {
            yield return Segment.Control($"\u001b[{i + 1};1H");
            foreach (Span span in _rows[i])
            {
                yield return new Segment(span.Text, _theme.StyleFor(span.Role));
            }
        }
    }
}
