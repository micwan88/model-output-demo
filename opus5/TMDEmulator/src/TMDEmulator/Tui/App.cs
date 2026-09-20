namespace TMDEmulator.Tui;

/// <summary>Main loop: keeps the screen navigation stack, routes keys, and redraws after every key or resize.</summary>
internal sealed class App
{
    private readonly ITerminal _terminal;
    private readonly Stack<IScreen> _screens = new();

    public App(ITerminal terminal, IScreen root, StatusMessage? initialMessage = null)
    {
        _terminal = terminal;
        _screens.Push(root);
        Message = initialMessage;
    }

    public IScreen Current => _screens.Peek();

    public int Depth => _screens.Count;

    public StatusMessage? Message { get; private set; }

    /// <summary>Runs until an Exit result or Ctrl+C.</summary>
    public void Run()
    {
        Draw();
        while (true)
        {
            ConsoleKeyInfo? input = _terminal.WaitForInput();
            if (input is ConsoleKeyInfo key)
            {
                if (IsCtrlC(key))
                {
                    return;
                }

                if (!ScreenFrame.IsTooSmall(_terminal.Width, _terminal.Height) && !Handle(key))
                {
                    return;
                }

                // Coalesce queued keys (e.g. a pasted path) into a single redraw.
                if (_terminal.KeyAvailable)
                {
                    continue;
                }
            }

            Draw();
        }
    }

    /// <returns>false when the app should exit.</returns>
    private bool Handle(ConsoleKeyInfo key)
    {
        Message = null;
        ScreenResult result;
        try
        {
            result = Current.HandleKey(key);
        }
        catch (Exception ex)
        {
            // Story: show any error in red on the status bar rather than crashing the emulator.
            result = ScreenResult.Stay(StatusMessage.Error($"Error: {ex.Message}"));
        }

        switch (result.Action)
        {
            case NavAction.Exit:
                return false;
            case NavAction.Push:
                _screens.Push(result.Next!);
                break;
            case NavAction.Pop when _screens.Count > 1:
                _screens.Pop();
                break;
        }

        Message = result.Message;
        return true;
    }

    private void Draw()
    {
        string breadcrumb = string.Join(" › ", _screens.Reverse().Select(s => s.Title));
        _terminal.Draw(ScreenFrame.Compose(breadcrumb, Current.Render(), Current.Legend, Message, _terminal.Width, _terminal.Height));
    }

    private static bool IsCtrlC(ConsoleKeyInfo key) =>
        key.Key == ConsoleKey.C && key.Modifiers.HasFlag(ConsoleModifiers.Control);
}
