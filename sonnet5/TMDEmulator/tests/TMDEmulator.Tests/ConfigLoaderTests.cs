using TMDEmulator.Config;
using TMDEmulator.Terminal;
using TMDEmulator.Tests.Support;

namespace TMDEmulator.Tests;

public sealed class ConfigLoaderTests : IDisposable
{
    private readonly TempDirectory _dir = new();

    public void Dispose() => _dir.Dispose();

    private AppConfig LoadJson(string json)
    {
        var path = _dir.Combine("tmdemulator.json");
        File.WriteAllText(path, json);
        return ConfigLoader.Load(path);
    }

    [Fact]
    public void Default_theme_matches_the_story_colors()
    {
        var t = Theme.Default;
        Assert.Equal(ConsoleColor.Black, t.Background);
        Assert.Equal(ConsoleColor.White, t.Text);
        Assert.Equal(ConsoleColor.Gray, t.HighlightBackground);
        Assert.Equal(ConsoleColor.Black, t.HighlightText);
        Assert.Equal(ConsoleColor.Red, t.Error);
    }

    [Fact]
    public void Missing_file_gives_defaults_and_no_errors()
    {
        var config = ConfigLoader.Load(_dir.Combine("nope.json"));

        Assert.Equal(Theme.Default, config.Theme);
        Assert.Empty(config.Errors);
    }

    [Fact]
    public void Shipped_config_file_loads_cleanly_and_equals_the_defaults()
    {
        var shipped = Path.Combine(AppContext.BaseDirectory, "tmdemulator.json");
        Assert.True(File.Exists(shipped), "tmdemulator.json must be copied next to the binaries");

        var config = ConfigLoader.Load(shipped);

        Assert.Empty(config.Errors);
        Assert.Equal(Theme.Default, config.Theme);
    }

    [Fact]
    public void Valid_overrides_are_applied_and_names_are_case_insensitive()
    {
        var config = LoadJson("""
            { "colors": { "background": "darkblue", "TEXT": "Yellow", "highlightBackground": "White",
                          "highlightText": "DarkGray", "error": "Magenta" } }
            """);

        Assert.Empty(config.Errors);
        Assert.Equal(new Theme(ConsoleColor.DarkBlue, ConsoleColor.Yellow, ConsoleColor.White,
            ConsoleColor.DarkGray, ConsoleColor.Magenta), config.Theme);
    }

    [Fact]
    public void Partial_config_keeps_defaults_for_the_rest()
    {
        var config = LoadJson("""{ "colors": { "error": "Yellow" } }""");

        Assert.Empty(config.Errors);
        Assert.Equal(Theme.Default with { Error = ConsoleColor.Yellow }, config.Theme);
    }

    [Fact]
    public void Missing_colors_section_is_fine()
    {
        var config = LoadJson("{}");

        Assert.Empty(config.Errors);
        Assert.Equal(Theme.Default, config.Theme);
    }

    [Fact]
    public void Comments_and_trailing_commas_are_tolerated()
    {
        var config = LoadJson("""
            {
              // a comment
              "colors": { "error": "Yellow", },
            }
            """);

        Assert.Empty(config.Errors);
        Assert.Equal(ConsoleColor.Yellow, config.Theme.Error);
    }

    [Fact]
    public void Unknown_color_name_is_reported_and_default_kept()
    {
        var config = LoadJson("""{ "colors": { "error": "Purplish", "text": "Green" } }""");

        var error = Assert.Single(config.Errors);
        Assert.Contains("'error'", error);
        Assert.Equal(ConsoleColor.Red, config.Theme.Error);
        Assert.Equal(ConsoleColor.Green, config.Theme.Text);
    }

    [Theory]
    [InlineData("""{ "colors": { "error": "12" } }""")]
    [InlineData("""{ "colors": { "error": 12 } }""")]
    [InlineData("""{ "colors": { "error": null } }""")]
    [InlineData("""{ "colors": { "error": "" } }""")]
    public void Numbers_and_non_strings_are_not_accepted_as_colors(string json)
    {
        var config = LoadJson(json);

        Assert.Single(config.Errors);
        Assert.Equal(ConsoleColor.Red, config.Theme.Error);
    }

    [Fact]
    public void Unknown_setting_name_is_reported()
    {
        var config = LoadJson("""{ "colors": { "errorr": "Red" } }""");

        var error = Assert.Single(config.Errors);
        Assert.Contains("errorr", error);
        Assert.Equal(Theme.Default, config.Theme);
    }

    [Fact]
    public void Malformed_json_falls_back_to_defaults_with_an_error()
    {
        var config = LoadJson("{ this is not json");

        Assert.Equal(Theme.Default, config.Theme);
        var error = Assert.Single(config.Errors);
        Assert.Contains("not valid JSON", error);
    }

    [Theory]
    [InlineData("[]")]
    [InlineData("42")]
    public void Top_level_that_is_not_an_object_is_reported(string json)
    {
        var config = LoadJson(json);

        Assert.Equal(Theme.Default, config.Theme);
        Assert.Single(config.Errors);
    }

    [Fact]
    public void Colors_section_that_is_not_an_object_is_reported()
    {
        var config = LoadJson("""{ "colors": "Red" }""");

        Assert.Equal(Theme.Default, config.Theme);
        Assert.Single(config.Errors);
    }

    [Fact]
    public void A_file_that_cannot_be_read_is_reported_not_thrown()
    {
        var path = _dir.Combine("tmdemulator.json");
        File.WriteAllText(path, "{}");
        using var exclusiveLock = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

        var config = ConfigLoader.Load(path);

        Assert.Equal(Theme.Default, config.Theme);
        var error = Assert.Single(config.Errors);
        Assert.Contains("cannot be read", error);
    }
}
