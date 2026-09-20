using TMDEmulator.Main.Models;
using TMDEmulator.Main.UI.Terminal;

namespace TMDEmulator.Main.UI.Screens;

public class KeyManagementScreen : BaseScreen
{
    private MenuScreen _menuScreen;

    public KeyManagementScreen(TerminalRenderer renderer, ColorScheme colorScheme, BaseScreen parentScreen)
        : base(renderer, colorScheme, parentScreen)
    {
        var items = new List<MenuItem>
        {
            new("View Keys"),
            new("Back")
        };

        _menuScreen = new MenuScreen("Key Management", items);
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
                0 => new PlaceholderScreen(Renderer, ColorScheme, this, "View Keys"),
                1 => ParentScreen,
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
