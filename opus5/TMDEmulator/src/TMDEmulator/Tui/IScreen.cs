namespace TMDEmulator.Tui;

internal interface IScreen
{
    /// <summary>Title used in the Main frame breadcrumb.</summary>
    string Title { get; }

    /// <summary>Navigation legend shown in the status bar.</summary>
    string Legend { get; }

    IReadOnlyList<BodyLine> Render();

    ScreenResult HandleKey(ConsoleKeyInfo key);
}
