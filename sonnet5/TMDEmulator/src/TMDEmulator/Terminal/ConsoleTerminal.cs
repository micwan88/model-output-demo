using System.Diagnostics;
using System.Text;

namespace TMDEmulator.Terminal;

/// <summary>
/// <see cref="ITerminal"/> over <see cref="Console"/>. Deliberately thin and free of logic: it is the one
/// class not covered by unit tests (it needs a real terminal), so it is checked by a manual smoke run.
/// Uses the 16 <see cref="ConsoleColor"/>s so no ANSI/VT setup is needed on Windows.
/// </summary>
public sealed class ConsoleTerminal : ITerminal
{
    public int Width => Console.WindowWidth;
    public int Height => Console.WindowHeight;

    public void Initialize()
    {
        Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        Console.TreatControlCAsInput = true;
        Console.CursorVisible = false;
    }

    public void Restore()
    {
        Console.ResetColor();
        Console.Clear();
        Console.CursorVisible = true;
        Console.TreatControlCAsInput = false;
    }

    public void Clear(ConsoleColor background)
    {
        Console.BackgroundColor = background;
        Console.Clear();
    }

    public void Write(int row, int column, string text, ConsoleColor foreground, ConsoleColor background)
    {
        Console.SetCursorPosition(column, row);
        Console.ForegroundColor = foreground;
        Console.BackgroundColor = background;
        Console.Write(text);
    }

    public bool WaitForKey(int timeoutMs)
    {
        var timer = Stopwatch.StartNew();
        while (!Console.KeyAvailable)
        {
            if (timer.ElapsedMilliseconds >= timeoutMs)
                return false;
            Thread.Sleep(10);
        }

        return true;
    }

    public ConsoleKeyInfo ReadKey() => Console.ReadKey(intercept: true);
}
