namespace TMDEmulator.Tests.Support;

internal static class Keys
{
    public static ConsoleKeyInfo Up => Of(ConsoleKey.UpArrow);
    public static ConsoleKeyInfo Down => Of(ConsoleKey.DownArrow);
    public static ConsoleKeyInfo Left => Of(ConsoleKey.LeftArrow);
    public static ConsoleKeyInfo Right => Of(ConsoleKey.RightArrow);
    public static ConsoleKeyInfo Enter => new('\r', ConsoleKey.Enter, false, false, false);
    public static ConsoleKeyInfo Esc => new('\u001b', ConsoleKey.Escape, false, false, false);
    public static ConsoleKeyInfo Backspace => new('\b', ConsoleKey.Backspace, false, false, false);
    public static ConsoleKeyInfo Delete => Of(ConsoleKey.Delete);
    public static ConsoleKeyInfo Home => Of(ConsoleKey.Home);
    public static ConsoleKeyInfo End => Of(ConsoleKey.End);
    public static ConsoleKeyInfo Tab => new('\t', ConsoleKey.Tab, false, false, false);
    public static ConsoleKeyInfo CtrlC => new('\u0003', ConsoleKey.C, false, false, true);

    public static ConsoleKeyInfo Of(ConsoleKey key) => new('\0', key, false, false, false);

    public static ConsoleKeyInfo Char(char c) =>
        new(c, System.Char.IsLetter(c) ? Enum.Parse<ConsoleKey>(System.Char.ToUpperInvariant(c).ToString()) : ConsoleKey.Oem1, System.Char.IsUpper(c), false, false);

    public static IEnumerable<ConsoleKeyInfo> Text(string text) => text.Select(Char);
}
