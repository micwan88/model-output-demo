namespace TMDEmulator.Main.UI.Terminal;

public class TerminalInput
{
    public static ConsoleKeyInfo ReadKey()
    {
        return Console.ReadKey(true);
    }

    public static string? ReadLine(string prompt = "")
    {
        if (!string.IsNullOrEmpty(prompt))
        {
            Console.Write(prompt);
        }

        return Console.ReadLine();
    }

    public static bool IsArrowUp(ConsoleKey key)
    {
        return key == ConsoleKey.UpArrow;
    }

    public static bool IsArrowDown(ConsoleKey key)
    {
        return key == ConsoleKey.DownArrow;
    }

    public static bool IsEnter(ConsoleKey key)
    {
        return key == ConsoleKey.Enter;
    }

    public static bool IsEscape(ConsoleKey key)
    {
        return key == ConsoleKey.Escape;
    }

    public static void ClearScreen()
    {
        Console.Clear();
    }

    public static (int Width, int Height) GetTerminalSize()
    {
        return (Console.WindowWidth, Console.WindowHeight);
    }
}
