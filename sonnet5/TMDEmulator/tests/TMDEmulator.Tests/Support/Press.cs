namespace TMDEmulator.Tests.Support;

public static class Press
{
    public static ConsoleKeyInfo Up => Special(ConsoleKey.UpArrow);
    public static ConsoleKeyInfo Down => Special(ConsoleKey.DownArrow);
    public static ConsoleKeyInfo Left => Special(ConsoleKey.LeftArrow);
    public static ConsoleKeyInfo Right => Special(ConsoleKey.RightArrow);
    public static ConsoleKeyInfo Home => Special(ConsoleKey.Home);
    public static ConsoleKeyInfo End => Special(ConsoleKey.End);
    public static ConsoleKeyInfo Delete => Special(ConsoleKey.Delete);
    public static ConsoleKeyInfo Backspace => new('\b', ConsoleKey.Backspace, false, false, false);
    public static ConsoleKeyInfo Enter => new('\r', ConsoleKey.Enter, false, false, false);
    public static ConsoleKeyInfo Escape => new('\u001b', ConsoleKey.Escape, false, false, false);
    public static ConsoleKeyInfo CtrlC => new('\u0003', ConsoleKey.C, false, false, true);
    public static ConsoleKeyInfo Tab => new('\t', ConsoleKey.Tab, false, false, false);

    public static ConsoleKeyInfo Char(char c) => new(c, default, false, false, false);

    public static ConsoleKeyInfo[] Text(string text) => text.Select(Char).ToArray();

    /// <summary>A key that produces no character (e.g. an arrow or function key).</summary>
    private static ConsoleKeyInfo Special(ConsoleKey key) => new('\0', key, false, false, false);
}
