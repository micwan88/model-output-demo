namespace TMDEmulator.Config;

/// <summary>"Theme" section of appsettings.json. Each value is a colour name (e.g. "grey") or "#RRGGBB".</summary>
internal sealed class ThemeConfig
{
    public string? Background { get; set; }
    public string? Foreground { get; set; }
    public string? Border { get; set; }
    public string? HighlightBackground { get; set; }
    public string? HighlightForeground { get; set; }
    public string? StatusBarBackground { get; set; }
    public string? StatusBarForeground { get; set; }
    public string? Error { get; set; }
    public string? Success { get; set; }
}

internal sealed class AppSettings
{
    public ThemeConfig? Theme { get; set; }
}
