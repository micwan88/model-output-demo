namespace TMDEmulator.Tui;

/// <summary>Terminal input/output used by <see cref="App"/>; faked in unit tests.</summary>
internal interface ITerminal
{
    int Width { get; }

    int Height { get; }

    /// <summary>True when more key presses are already queued (e.g. pasted text).</summary>
    bool KeyAvailable { get; }

    /// <summary>Blocks until a key is pressed (returns the key) or the terminal is resized (returns null).</summary>
    ConsoleKeyInfo? WaitForInput();

    /// <summary>Draws a full screen of rows, as composed by <see cref="ScreenFrame"/>.</summary>
    void Draw(IReadOnlyList<IReadOnlyList<Span>> rows);
}
