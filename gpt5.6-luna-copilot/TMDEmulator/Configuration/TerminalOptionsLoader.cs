using System.Text.Json;

namespace TMDEmulator.Configuration;

public static class TerminalOptionsLoader
{
    public static TerminalOptions Load(string path)
    {
        if (!File.Exists(path))
        {
            return new TerminalOptions();
        }

        try
        {
            var json = File.ReadAllText(path);
            var options = JsonSerializer.Deserialize<TerminalOptions>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (options is null)
            {
                throw new ConfigurationException("The configuration file is empty.");
            }

            Validate(options);
            return options;
        }
        catch (JsonException exception)
        {
            throw new ConfigurationException(
                $"The configuration file is not valid JSON: {exception.Message}",
                exception);
        }
        catch (IOException exception)
        {
            throw new ConfigurationException(
                $"The configuration file could not be read: {exception.Message}",
                exception);
        }
        catch (UnauthorizedAccessException exception)
        {
            throw new ConfigurationException(
                $"The configuration file could not be read: {exception.Message}",
                exception);
        }
    }

    public static ResolvedTerminalColors ResolveColors(TerminalOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        Validate(options);

        return new ResolvedTerminalColors(
            ParseColor(options.Colors.Background),
            ParseColor(options.Colors.Foreground),
            ParseColor(options.Colors.HighlightBackground),
            ParseColor(options.Colors.HighlightForeground),
            ParseColor(options.Colors.FrameForeground),
            ParseColor(options.Colors.ErrorForeground));
    }

    private static void Validate(TerminalOptions options)
    {
        if (options.Colors is null)
        {
            throw new ConfigurationException("The colors configuration is missing.");
        }

        _ = ParseColor(options.Colors.Background);
        _ = ParseColor(options.Colors.Foreground);
        _ = ParseColor(options.Colors.HighlightBackground);
        _ = ParseColor(options.Colors.HighlightForeground);
        _ = ParseColor(options.Colors.FrameForeground);
        _ = ParseColor(options.Colors.ErrorForeground);
    }

    private static ConsoleColor ParseColor(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            !Enum.TryParse<ConsoleColor>(value, true, out var color))
        {
            throw new ConfigurationException(
                $"'{value}' is not a valid ConsoleColor value.");
        }

        return color;
    }
}

public sealed class ConfigurationException : Exception
{
    public ConfigurationException(string message)
        : base(message)
    {
    }

    public ConfigurationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
