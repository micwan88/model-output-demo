using Spectre.Console;
using Spectre.Console.Rendering;
using Spectre.Console.Testing;
using TMDEmulator.Tui;

namespace TMDEmulator.Tests.Tui;

public class FrameRenderableTests
{
    private static readonly IReadOnlyList<IReadOnlyList<Span>> Rows = new[]
    {
        new[] { new Span("Menu"), new Span("  ") },
        new[] { new Span("> Item", Role.Highlight) },
    };

    [Fact]
    public void Render_PositionsEachRowAndAppliesThemeColours()
    {
        var console = new TestConsole { EmitAnsiSequences = true };
        console.Profile.Capabilities.ColorSystem = ColorSystem.Standard;

        console.Write(new FrameRenderable(Rows, Theme.Default));

        string output = console.Output;
        Assert.Contains("\u001b[1;1H", output);
        Assert.Contains("\u001b[2;1H", output);
        Assert.True(output.IndexOf("\u001b[2;1H", StringComparison.Ordinal) < output.IndexOf("> Item", StringComparison.Ordinal));
        Assert.Contains("\u001b[30;47m> Item", output); // black text on grey (silver) highlight
    }

    [Fact]
    public void Render_WithoutAnsi_WritesPlainText()
    {
        var console = new TestConsole();

        console.Write(new FrameRenderable(Rows, Theme.Default));

        Assert.Equal("Menu  > Item", console.Output);
    }

    [Fact]
    public void Measure_UsesFullWidth()
    {
        var console = new TestConsole();
        var renderable = new FrameRenderable(Rows, Theme.Default);

        Measurement m = ((IRenderable)renderable).Measure(new RenderOptions(console.Profile.Capabilities, new Size(80, 24)), 80);

        Assert.Equal(80, m.Min);
        Assert.Equal(80, m.Max);
    }
}
