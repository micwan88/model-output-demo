namespace TMDEmulator.Terminal;

/// <summary>The only surface the UI needs from a terminal. Kept tiny so it can be faked in tests.</summary>
public interface ITerminal
{
    int Width { get; }
    int Height { get; }

    /// <summary>Prepare the terminal for full-screen use (hide cursor, deliver Ctrl+C as a key).</summary>
    void Initialize();

    /// <summary>Undo <see cref="Initialize"/> and leave the console clean for the shell.</summary>
    void Restore();

    /// <summary>Fill the whole terminal with the given background color.</summary>
    void Clear(ConsoleColor background);

    /// <summary>Write text at a position. Never called with text that would pass the last column.</summary>
    void Write(int row, int column, string text, ConsoleColor foreground, ConsoleColor background);

    /// <summary>Wait up to <paramref name="timeoutMs"/> for a key. False means "no key yet".</summary>
    bool WaitForKey(int timeoutMs);

    ConsoleKeyInfo ReadKey();
}
