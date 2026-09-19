using System.Text.Json;
using Spectre.Console;
using TMDEmulator.Tui;

namespace TMDEmulator.Config;

internal static class ConfigLoader
{
    public const string FileName = "appsettings.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    /// <summary>
    /// Loads the colour theme. Never throws: a missing file or key falls back to the default silently;
    /// a malformed file or an invalid colour falls back to the default and returns a warning to display.
    /// </summary>
    public static (Theme Theme, string? Warning) LoadTheme(string path)
    {
        if (!File.Exists(path))
        {
            return (Theme.Default, null);
        }

        AppSettings? settings;
        try
        {
            settings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path), JsonOptions);
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            return (Theme.Default, $"{Path.GetFileName(path)} could not be read; default colours used. {ex.Message}");
        }

        ThemeConfig? config = settings?.Theme;
        if (config is null)
        {
            return (Theme.Default, null);
        }

        var invalidKeys = new List<string>();
        Color Resolve(string? value, Color fallback, string key)
        {
            if (value is null)
            {
                return fallback;
            }

            if (TryParseColor(value, out Color color))
            {
                return color;
            }

            invalidKeys.Add(key);
            return fallback;
        }

        Theme d = Theme.Default;
        var theme = new Theme
        {
            Background = Resolve(config.Background, d.Background, nameof(config.Background)),
            Foreground = Resolve(config.Foreground, d.Foreground, nameof(config.Foreground)),
            Border = Resolve(config.Border, d.Border, nameof(config.Border)),
            HighlightBackground = Resolve(config.HighlightBackground, d.HighlightBackground, nameof(config.HighlightBackground)),
            HighlightForeground = Resolve(config.HighlightForeground, d.HighlightForeground, nameof(config.HighlightForeground)),
            StatusBarBackground = Resolve(config.StatusBarBackground, d.StatusBarBackground, nameof(config.StatusBarBackground)),
            StatusBarForeground = Resolve(config.StatusBarForeground, d.StatusBarForeground, nameof(config.StatusBarForeground)),
            Error = Resolve(config.Error, d.Error, nameof(config.Error)),
            Success = Resolve(config.Success, d.Success, nameof(config.Success)),
        };

        string? warning = invalidKeys.Count == 0
            ? null
            : $"Invalid colour in config for Theme.{string.Join(", Theme.", invalidKeys)}; default used.";
        return (theme, warning);
    }

    /// <summary>Accepts a single colour token: a Spectre.Console colour name or #RRGGBB.</summary>
    internal static bool TryParseColor(string value, out Color color)
    {
        color = Color.Default;
        string token = value.Trim();
        if (token.Length == 0 || token.Contains(' ') || !Style.TryParse(token, out Style style))
        {
            return false;
        }

        // Decoration words such as "bold" parse successfully but carry no colour.
        if (style.Foreground == Color.Default)
        {
            return false;
        }

        color = style.Foreground;
        return true;
    }
}
