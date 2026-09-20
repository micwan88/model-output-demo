using System.Diagnostics.CodeAnalysis;
using Spectre.Console;

namespace TMDEmulator.Tui;

/// <summary>
/// Real console implementation of <see cref="ITerminal"/>. Thin I/O shim over System.Console and
/// Spectre.Console; excluded from coverage and verified by the interactive smoke test instead.
/// </summary>
[ExcludeFromCodeCoverage]
internal sealed class SystemTerminal : ITerminal
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(25);

    private readonly IAnsiConsole _console;
    private readonly Theme _theme;
    private int _width;
    private int _height;

    public SystemTerminal(IAnsiConsole console, Theme theme)
    {
        _console = console;
        _theme = theme;
        (_width, _height) = (Console.WindowWidth, Console.WindowHeight);
    }

    public int Width => _width;

    public int Height => _height;

    public bool KeyAvailable => Console.KeyAvailable;

    /// <summary>Runs <paramref name="body"/> in the alternate screen with the cursor hidden, restoring the terminal afterwards.</summary>
    public void Run(Action body)
    {
        bool treatControlCAsInput = Console.TreatControlCAsInput;
        Console.TreatControlCAsInput = true;
        try
        {
            _console.AlternateScreen(() =>
            {
                _console.Cursor.Hide();
                try
                {
                    body();
                }
                finally
                {
                    _console.Cursor.Show();
                }
            });
        }
        finally
        {
            Console.TreatControlCAsInput = treatControlCAsInput;
        }
    }

    public ConsoleKeyInfo? WaitForInput()
    {
        while (true)
        {
            if (Console.KeyAvailable)
            {
                return Console.ReadKey(intercept: true);
            }

            if (Console.WindowWidth != _width || Console.WindowHeight != _height)
            {
                (_width, _height) = (Console.WindowWidth, Console.WindowHeight);
                return null;
            }

            Thread.Sleep(PollInterval);
        }
    }

    public void Draw(IReadOnlyList<IReadOnlyList<Span>> rows) => _console.Write(new FrameRenderable(rows, _theme));
}
