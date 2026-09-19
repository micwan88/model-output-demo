using Spectre.Console;
using TMDEmulator.Tui;

namespace TMDEmulator.Tests.Tui;

public class ThemeTests
{
    [Fact]
    public void Default_FollowsStoryColours()
    {
        Theme theme = Theme.Default;

        Assert.Equal(Color.Black, theme.Background);
        Assert.Equal(Color.White, theme.Foreground);
        Assert.Equal(Color.Silver, theme.HighlightBackground); // #C0C0C0 grey
        Assert.Equal(Color.Black, theme.HighlightForeground);
        Assert.Equal(Color.Red, theme.Error);
    }

    [Fact]
    public void StyleFor_MapsEachRole()
    {
        Theme t = Theme.Default with
        {
            Border = Color.Blue,
            HighlightBackground = Color.Yellow,
            StatusBarBackground = Color.Navy,
            StatusBarForeground = Color.Aqua,
            Success = Color.Lime,
        };

        foreach (Role role in Enum.GetValues<Role>())
        {
            Assert.Equal(Expected(role), t.StyleFor(role));
        }
    }

    private static Style Expected(Role role) => role switch
    {
        Role.Highlight => new Style(Color.Black, Color.Yellow),
        Role.Border => new Style(Color.Blue, Color.Black),
        Role.Title => new Style(Color.White, Color.Black, Decoration.Bold),
        Role.StatusBar => new Style(Color.Aqua, Color.Navy),
        Role.Error => new Style(Color.Red, Color.Navy),
        Role.Success => new Style(Color.Lime, Color.Navy),
        _ => new Style(Color.White, Color.Black),
    };
}
