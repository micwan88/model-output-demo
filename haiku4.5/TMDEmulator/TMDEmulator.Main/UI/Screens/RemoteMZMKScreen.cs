using TMDEmulator.Main.Models;
using TMDEmulator.Main.UI.Terminal;

namespace TMDEmulator.Main.UI.Screens;

public class RemoteMZMKScreen : BaseScreen
{
    private MenuScreen _menuScreen;

    public RemoteMZMKScreen(TerminalRenderer renderer, ColorScheme colorScheme, BaseScreen parentScreen)
        : base(renderer, colorScheme, parentScreen)
    {
        var items = new List<MenuItem>
        {
            new("Export MZMK Initialization Key"),
            new("Finalize Remote MZMK"),
            new("Uninstalling a MZMK"),
            new("Back")
        };

        _menuScreen = new MenuScreen("Remote MZMK Setup", items);
    }

    public override void Render()
    {
        var displayItems = _menuScreen.GetDisplayItems();
        Renderer.RenderMenu(_menuScreen.Title, displayItems, GetStatusBarText());
    }

    public override BaseScreen? HandleInput(ConsoleKeyInfo keyInfo)
    {
        if (TerminalInput.IsArrowUp(keyInfo.Key))
        {
            _menuScreen.MoveUp();
            Render();
            return this;
        }

        if (TerminalInput.IsArrowDown(keyInfo.Key))
        {
            _menuScreen.MoveDown();
            Render();
            return this;
        }

        if (TerminalInput.IsEnter(keyInfo.Key))
        {
            var selectedIndex = _menuScreen.GetSelectedIndex();
            return selectedIndex switch
            {
                0 => new ExportMZMKScreen(Renderer, ColorScheme, this),
                1 => new PlaceholderScreen(Renderer, ColorScheme, this, "Finalize Remote MZMK"),
                2 => new PlaceholderScreen(Renderer, ColorScheme, this, "Uninstalling a MZMK"),
                3 => ParentScreen,
                _ => this
            };
        }

        if (TerminalInput.IsEscape(keyInfo.Key))
        {
            return ParentScreen;
        }

        return this;
    }

    public override string GetStatusBarText()
    {
        return "↑↓ Navigate | Enter Select | Esc Back";
    }
}
