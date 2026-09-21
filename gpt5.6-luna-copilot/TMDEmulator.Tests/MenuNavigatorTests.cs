using TMDEmulator.UI;

namespace TMDEmulator.Tests;

public sealed class MenuNavigatorTests
{
    [Fact]
    public void SelectionWrapsInBothDirections()
    {
        var navigator = new MenuNavigator(
            new MenuScreen(
                "Test",
                new[]
                {
                    new MenuItem("One"),
                    new MenuItem("Two")
                }));

        navigator.MoveSelection(-1);
        Assert.Equal(1, navigator.SelectedIndex);

        navigator.MoveSelection(1);
        Assert.Equal(0, navigator.SelectedIndex);
    }

    [Fact]
    public void PushAndBackRestoreParentScreen()
    {
        var root = MenuDefinitions.CreateRoot();
        var navigator = new MenuNavigator(root);
        navigator.Push(MenuDefinitions.CreateRemoteMzmkSetup());

        Assert.Equal("Remote MZMK Setup", navigator.CurrentScreen.Title);
        Assert.True(navigator.GoBack());
        Assert.Equal(root, navigator.CurrentScreen);
        Assert.False(navigator.GoBack());
    }

    [Fact]
    public void ActivationReturnsSelectedItem()
    {
        var navigator = new MenuNavigator(MenuDefinitions.CreateKeyManagement());
        navigator.MoveSelection(1);

        var selected = navigator.Activate();

        Assert.Equal("Back", selected.Label);
        Assert.Equal(MenuAction.Back, selected.Action);
    }
}
