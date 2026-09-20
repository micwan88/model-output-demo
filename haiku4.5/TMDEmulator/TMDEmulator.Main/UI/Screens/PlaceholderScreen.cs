using TMDEmulator.Main.UI.Terminal;
using Spectre.Console;

namespace TMDEmulator.Main.UI.Screens;

public class PlaceholderScreen : BaseScreen
{
    private string _featureName;

    public PlaceholderScreen(TerminalRenderer renderer, ColorScheme colorScheme, BaseScreen parentScreen, string featureName)
        : base(renderer, colorScheme, parentScreen)
    {
        _featureName = featureName;
    }

    public override void Render()
    {
        var content = $"[yellow]This feature ({_featureName}) will be available in a future update.[/]\n\nPress any key to return to the previous menu.";
        Renderer.RenderMainFrame(content, GetStatusBarText());
    }

    public override BaseScreen? HandleInput(ConsoleKeyInfo keyInfo)
    {
        return ParentScreen;
    }

    public override string GetStatusBarText()
    {
        return "Press any key to return";
    }
}
