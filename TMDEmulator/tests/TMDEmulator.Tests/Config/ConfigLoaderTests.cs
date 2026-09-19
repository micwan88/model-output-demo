using Spectre.Console;
using TMDEmulator.Config;
using TMDEmulator.Tests.Support;
using TMDEmulator.Tui;

namespace TMDEmulator.Tests.Config;

public sealed class ConfigLoaderTests : IDisposable
{
    private readonly TempDirectory _temp = new();

    public void Dispose() => _temp.Dispose();

    private string WriteConfig(string json)
    {
        string path = _temp.Combine(ConfigLoader.FileName);
        File.WriteAllText(path, json);
        return path;
    }

    [Fact]
    public void LoadTheme_MissingFile_ReturnsDefaultsWithoutWarning()
    {
        (Theme theme, string? warning) = ConfigLoader.LoadTheme(_temp.Combine("missing.json"));

        Assert.Equal(Theme.Default, theme);
        Assert.Null(warning);
    }

    [Fact]
    public void LoadTheme_MalformedJson_ReturnsDefaultsWithWarning()
    {
        (Theme theme, string? warning) = ConfigLoader.LoadTheme(WriteConfig("{ \"Theme\": { "));

        Assert.Equal(Theme.Default, theme);
        Assert.NotNull(warning);
        Assert.StartsWith("appsettings.json could not be read; default colours used.", warning);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{ \"Other\": 1 }")]
    [InlineData("null")]
    public void LoadTheme_NoThemeSection_ReturnsDefaultsWithoutWarning(string json)
    {
        (Theme theme, string? warning) = ConfigLoader.LoadTheme(WriteConfig(json));

        Assert.Equal(Theme.Default, theme);
        Assert.Null(warning);
    }

    [Fact]
    public void LoadTheme_ValidValues_AreApplied_AndMissingKeysKeepDefaults()
    {
        string path = WriteConfig("""
            {
              // comments and trailing commas are tolerated
              "theme": { "Background": "navy", "HighlightBackground": "#AABBCC", },
            }
            """);

        (Theme theme, string? warning) = ConfigLoader.LoadTheme(path);

        Assert.Null(warning);
        Assert.Equal(Color.Navy, theme.Background);
        Assert.Equal(new Color(0xAA, 0xBB, 0xCC), theme.HighlightBackground);
        Assert.Equal(Theme.Default.Foreground, theme.Foreground);
    }

    [Fact]
    public void LoadTheme_AllKeys_AreRead()
    {
        string path = WriteConfig("""
            { "Theme": {
                "Background": "#000001", "Foreground": "#000002", "Border": "#000003",
                "HighlightBackground": "#000004", "HighlightForeground": "#000005",
                "StatusBarBackground": "#000006", "StatusBarForeground": "#000007",
                "Error": "#000008", "Success": "#000009" } }
            """);

        (Theme theme, _) = ConfigLoader.LoadTheme(path);

        Assert.Equal(new Color(0, 0, 1), theme.Background);
        Assert.Equal(new Color(0, 0, 2), theme.Foreground);
        Assert.Equal(new Color(0, 0, 3), theme.Border);
        Assert.Equal(new Color(0, 0, 4), theme.HighlightBackground);
        Assert.Equal(new Color(0, 0, 5), theme.HighlightForeground);
        Assert.Equal(new Color(0, 0, 6), theme.StatusBarBackground);
        Assert.Equal(new Color(0, 0, 7), theme.StatusBarForeground);
        Assert.Equal(new Color(0, 0, 8), theme.Error);
        Assert.Equal(new Color(0, 0, 9), theme.Success);
    }

    [Fact]
    public void LoadTheme_InvalidColour_UsesDefaultForThatKeyAndWarns()
    {
        string path = WriteConfig("""{ "Theme": { "Foreground": "not-a-colour", "Error": "bold", "Background": "blue" } }""");

        (Theme theme, string? warning) = ConfigLoader.LoadTheme(path);

        Assert.Equal(Theme.Default.Foreground, theme.Foreground);
        Assert.Equal(Theme.Default.Error, theme.Error);
        Assert.Equal(Color.Blue, theme.Background);
        Assert.Equal("Invalid colour in config for Theme.Foreground, Theme.Error; default used.", warning);
    }

    [Fact]
    public void ShippedAppSettings_LoadsWithoutWarningAndMatchesDefaults()
    {
        (Theme theme, string? warning) = ConfigLoader.LoadTheme(Path.Combine(AppContext.BaseDirectory, ConfigLoader.FileName));

        Assert.Null(warning);
        Assert.Equal(Theme.Default, theme);
    }

    [Theory]
    [InlineData("red", true)]
    [InlineData(" grey ", true)]
    [InlineData("#C0C0C0", true)]
    [InlineData("", false)]
    [InlineData("bold", false)]
    [InlineData("red on blue", false)]
    [InlineData("#GG0000", false)]
    public void TryParseColor(string value, bool expected)
    {
        Assert.Equal(expected, ConfigLoader.TryParseColor(value, out _));
    }
}
