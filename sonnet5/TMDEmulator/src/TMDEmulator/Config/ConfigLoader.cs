using System.Text.Json;
using TMDEmulator.Terminal;

namespace TMDEmulator.Config;

/// <summary>Loaded configuration. <see cref="Errors"/> lists problems found in the config file;
/// the affected values fell back to their defaults.</summary>
public sealed record AppConfig(Theme Theme, IReadOnlyList<string> Errors);

/// <summary>
/// Reads <c>tmdemulator.json</c>. A missing file is fine (defaults are used). Anything wrong inside the
/// file is reported in <see cref="AppConfig.Errors"/> and never stops the program from starting.
/// </summary>
public static class ConfigLoader
{
    private static readonly JsonDocumentOptions JsonOptions = new()
    {
        CommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    public static AppConfig Load(string path)
    {
        if (!File.Exists(path))
            return new AppConfig(Theme.Default, []);

        var file = Path.GetFileName(path);
        var errors = new List<string>();

        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(File.ReadAllText(path), JsonOptions);
        }
        catch (JsonException e)
        {
            return new AppConfig(Theme.Default, [$"{file}: not valid JSON ({e.Message}); using default colors"]);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            return new AppConfig(Theme.Default, [$"{file}: cannot be read ({e.Message}); using default colors"]);
        }

        using (doc)
        {
            var colors = ReadColors(doc.RootElement, file, errors);
            var theme = new Theme(
                colors["background"],
                colors["text"],
                colors["highlightBackground"],
                colors["highlightText"],
                colors["error"]);
            return new AppConfig(theme, errors);
        }
    }

    private static Dictionary<string, ConsoleColor> ReadColors(JsonElement root, string file, List<string> errors)
    {
        var d = Theme.Default;
        var colors = new Dictionary<string, ConsoleColor>(StringComparer.OrdinalIgnoreCase)
        {
            ["background"] = d.Background,
            ["text"] = d.Text,
            ["highlightBackground"] = d.HighlightBackground,
            ["highlightText"] = d.HighlightText,
            ["error"] = d.Error,
        };

        if (root.ValueKind != JsonValueKind.Object)
        {
            errors.Add($"{file}: top level must be a JSON object; using default colors");
            return colors;
        }

        if (!root.TryGetProperty("colors", out var section))
            return colors;

        if (section.ValueKind != JsonValueKind.Object)
        {
            errors.Add($"{file}: 'colors' must be an object; using default colors");
            return colors;
        }

        foreach (var entry in section.EnumerateObject())
        {
            if (!colors.ContainsKey(entry.Name))
            {
                errors.Add($"{file}: unknown color setting '{entry.Name}'");
                continue;
            }

            if (entry.Value.ValueKind == JsonValueKind.String && TryParseColor(entry.Value.GetString(), out var color))
                colors[entry.Name] = color;
            else
                errors.Add($"{file}: invalid color for '{entry.Name}' (using default {colors[entry.Name]})");
        }

        return colors;
    }

    /// <summary>Accepts <see cref="ConsoleColor"/> names only (case-insensitive); rejects numbers.</summary>
    private static bool TryParseColor(string? name, out ConsoleColor color)
    {
        foreach (var candidate in Enum.GetValues<ConsoleColor>())
        {
            if (string.Equals(candidate.ToString(), name, StringComparison.OrdinalIgnoreCase))
            {
                color = candidate;
                return true;
            }
        }

        color = default;
        return false;
    }
}
