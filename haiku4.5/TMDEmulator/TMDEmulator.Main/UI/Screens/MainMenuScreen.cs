using TMDEmulator.Main.Models;
using TMDEmulator.Main.UI.Terminal;

namespace TMDEmulator.Main.UI.Screens;

public class MainMenuScreen : BaseScreen
{
    private MenuScreen _menuScreen;

    public MainMenuScreen(TerminalRenderer renderer, ColorScheme colorScheme)
        : base(renderer, colorScheme)
    {
        var items = new List<MenuItem>
        {
            new("Remote MZMK Setup"),
            new("Key Management"),
            new("Exit")
        };

        _menuScreen = new MenuScreen("TMD Emulator - Main Menu", items);
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
                0 => new RemoteMZMKScreen(Renderer, ColorScheme, this),
                1 => new KeyManagementScreen(Renderer, ColorScheme, this),
                2 => null,
                _ => this
            };
        }

        if (TerminalInput.IsEscape(keyInfo.Key))
        {
            return null;
        }

        return this;
    }

    public override string GetStatusBarText()
    {
        return "↑↓ Navigate | Enter Select | Esc Exit";
    }
}
