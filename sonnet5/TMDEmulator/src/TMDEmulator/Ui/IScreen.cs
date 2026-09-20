namespace TMDEmulator.Ui;

/// <summary>One page of the emulator (a menu, a placeholder, a function screen).</summary>
public interface IScreen
{
    /// <summary>Shown in the breadcrumb at the top of the main frame.</summary>
    string Title { get; }

    /// <summary>Key legend shown in the status bar while this screen is active.</summary>
    string Legend { get; }

    void Render(Canvas canvas);

    void HandleKey(ConsoleKeyInfo key, ScreenHost host);
}
