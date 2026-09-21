using TMDEmulator.Configuration;

namespace TMDEmulator.Tests;

public sealed class ConfigurationTests
{
    [Fact]
    public void LoadMissingFileReturnsDefaults()
    {
        var options = TerminalOptionsLoader.Load(
            Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "missing.json"));

        var colors = TerminalOptionsLoader.ResolveColors(options);

        Assert.Equal(ConsoleColor.Black, colors.Background);
        Assert.Equal(ConsoleColor.White, colors.Foreground);
        Assert.Equal(ConsoleColor.Red, colors.ErrorForeground);
    }

    [Fact]
    public void LoadInvalidColorThrowsConfigurationException()
    {
        var directory = Directory.CreateTempSubdirectory();
        try
        {
            var path = Path.Combine(directory.FullName, "appsettings.json");
            File.WriteAllText(path, """{"colors":{"background":"NotAColor"}}""");

            var exception = Assert.Throws<ConfigurationException>(
                () => TerminalOptionsLoader.Load(path));

            Assert.Contains("NotAColor", exception.Message);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }
}
