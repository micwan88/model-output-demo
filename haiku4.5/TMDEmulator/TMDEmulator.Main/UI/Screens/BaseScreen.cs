using TMDEmulator.Main.UI.Terminal;

namespace TMDEmulator.Main.UI.Screens;

public abstract class BaseScreen
{
    protected TerminalRenderer Renderer { get; set; }
    protected ColorScheme ColorScheme { get; set; }
    protected BaseScreen? ParentScreen { get; set; }
    protected string? ErrorMessage { get; set; }

    public BaseScreen(TerminalRenderer renderer, ColorScheme colorScheme, BaseScreen? parentScreen = null)
    {
        Renderer = renderer;
        ColorScheme = colorScheme;
        ParentScreen = parentScreen;
    }

    public abstract void Render();
    public abstract BaseScreen? HandleInput(ConsoleKeyInfo keyInfo);
    public abstract string GetStatusBarText();

    public void SetError(string message)
    {
        ErrorMessage = message;
    }

    public void ClearError()
    {
        ErrorMessage = null;
    }

    public BaseScreen? GoBack()
    {
        return ParentScreen;
    }
}
