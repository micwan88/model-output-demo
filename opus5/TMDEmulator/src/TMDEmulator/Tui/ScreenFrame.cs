using System.Globalization;
using Spectre.Console.Rendering;

namespace TMDEmulator.Tui;

/// <summary>
/// Lays out a full screen: the Main frame (breadcrumb + body) above the Bottom frame
/// (status bar: navigation legend, then the message line). Every returned row is exactly
/// <c>width</c> cells so the configured background colour covers the whole terminal.
/// </summary>
internal static class ScreenFrame
{
    public const int MinWidth = 80;
    public const int MinHeight = 24;

    /// <summary>Rows used by the Bottom frame: top border, legend, message, bottom border.</summary>
    public const int StatusBarHeight = 4;

    public static bool IsTooSmall(int width, int height) => width < MinWidth || height < MinHeight;

    public static IReadOnlyList<IReadOnlyList<Span>> Compose(
        string breadcrumb, IReadOnlyList<BodyLine> body, string legend, StatusMessage? message, int width, int height)
    {
        if (IsTooSmall(width, height))
        {
            return ComposeTooSmall(width, height);
        }

        int innerWidth = width - 4;
        int bodyHeight = height - StatusBarHeight - 2;
        var rows = new List<IReadOnlyList<Span>>(height);

        // Main frame
        string title = Truncate(breadcrumb, width - 6);
        rows.Add(new[]
        {
            new Span("╭─ ", Role.Border),
            new Span(title, Role.Title),
            new Span(" " + new string('─', width - 5 - CellCount(title)) + "╮", Role.Border),
        });

        List<IReadOnlyList<Span>> bodyRows = body.SelectMany(line => Wrap(line, innerWidth)).ToList();
        if (bodyRows.Count > bodyHeight)
        {
            bodyRows = bodyRows.Take(bodyHeight - 1).Append(Pad(new[] { new Span("…") }, innerWidth, Role.Normal)).ToList();
        }

        for (int i = 0; i < bodyHeight; i++)
        {
            IReadOnlyList<Span> content = i < bodyRows.Count ? bodyRows[i] : Pad(Array.Empty<Span>(), innerWidth, Role.Normal);
            rows.Add(Boxed(content));
        }

        rows.Add(new[] { new Span("╰" + new string('─', width - 2) + "╯", Role.Border) });

        // Bottom frame (status bar)
        rows.Add(new[] { new Span("╭" + new string('─', width - 2) + "╮", Role.Border) });
        rows.Add(Boxed(Pad(new[] { new Span(Truncate(legend, innerWidth), Role.StatusBar) }, innerWidth, Role.StatusBar)));
        Role messageRole = message?.Kind == MessageKind.Error ? Role.Error : Role.Success;
        rows.Add(Boxed(Pad(new[] { new Span(Truncate(message?.Text ?? string.Empty, innerWidth), messageRole) }, innerWidth, Role.StatusBar)));
        rows.Add(new[] { new Span("╰" + new string('─', width - 2) + "╯", Role.Border) });

        return rows;
    }

    public static IReadOnlyList<IReadOnlyList<Span>> ComposeTooSmall(int width, int height)
    {
        string notice = $"Terminal too small ({width}x{height}). Please resize to at least {MinWidth}x{MinHeight}.";
        var rows = new List<IReadOnlyList<Span>>(Math.Max(height, 0));
        for (int i = 0; i < height; i++)
        {
            string text = i == 0 ? Truncate(notice, width) : string.Empty;
            rows.Add(Pad(new[] { new Span(text, Role.Error) }, width, Role.Normal));
        }

        return rows;
    }

    /// <summary>Splits a line into rows of at most <paramref name="width"/> cells, each padded with the line's fill role.</summary>
    internal static IEnumerable<IReadOnlyList<Span>> Wrap(BodyLine line, int width)
    {
        var current = new List<Span>();
        int used = 0;
        foreach (Span span in line.Spans)
        {
            var text = new System.Text.StringBuilder();
            foreach (string element in TextElements(span.Text))
            {
                int cells = CellCount(element);
                if (used + cells > width && used > 0)
                {
                    current.Add(span with { Text = text.ToString() });
                    yield return Pad(current, width, line.Fill);
                    current = new List<Span>();
                    text.Clear();
                    used = 0;
                }

                text.Append(element);
                used += cells;
            }

            current.Add(span with { Text = text.ToString() });
        }

        yield return Pad(current, width, line.Fill);
    }

    internal static string Truncate(string text, int maxCells)
    {
        if (CellCount(text) <= maxCells)
        {
            return text;
        }

        var result = new System.Text.StringBuilder();
        int used = 0;
        foreach (string element in TextElements(text))
        {
            int cells = CellCount(element);
            if (used + cells > maxCells - 1)
            {
                break;
            }

            result.Append(element);
            used += cells;
        }

        return maxCells > 0 ? result.Append('…').ToString() : string.Empty;
    }

    internal static int CellCount(string text) => new Segment(text).CellCount();

    private static IReadOnlyList<Span> Pad(IEnumerable<Span> spans, int width, Role fill)
    {
        var list = spans.Where(s => s.Text.Length > 0).ToList();
        int used = list.Sum(s => CellCount(s.Text));
        if (used < width)
        {
            list.Add(new Span(new string(' ', width - used), fill));
        }

        return list;
    }

    private static IReadOnlyList<Span> Boxed(IReadOnlyList<Span> content)
    {
        var row = new List<Span>(content.Count + 2) { new("│ ", Role.Border) };
        row.AddRange(content);
        row.Add(new Span(" │", Role.Border));
        return row;
    }

    private static IEnumerable<string> TextElements(string text)
    {
        TextElementEnumerator enumerator = StringInfo.GetTextElementEnumerator(text);
        while (enumerator.MoveNext())
        {
            yield return enumerator.GetTextElement();
        }
    }
}
