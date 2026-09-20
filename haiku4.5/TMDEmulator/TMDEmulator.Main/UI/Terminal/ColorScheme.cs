using Microsoft.Extensions.Configuration;
using Spectre.Console;

namespace TMDEmulator.Main.UI.Terminal;

public class ColorScheme
{
    private readonly IConfiguration _config;

    public string BackgroundColor { get; private set; } = "Black";
    public string TextColor { get; private set; } = "White";
    public string HighlightBackgroundColor { get; private set; } = "Grey23";
    public string HighlightTextColor { get; private set; } = "Black";
    public string ErrorColor { get; private set; } = "Red";
    public string StatusBarBackgroundColor { get; private set; } = "DarkSlateGray";

    public ColorScheme(IConfiguration config)
    {
        _config = config;
        LoadColors();
    }

    private void LoadColors()
    {
        BackgroundColor = _config["UI:Colors:BackgroundColor"] ?? "Black";
        TextColor = _config["UI:Colors:TextColor"] ?? "White";
        HighlightBackgroundColor = _config["UI:Colors:HighlightBackgroundColor"] ?? "Grey23";
        HighlightTextColor = _config["UI:Colors:HighlightTextColor"] ?? "Black";
        ErrorColor = _config["UI:Colors:ErrorColor"] ?? "Red";
        StatusBarBackgroundColor = _config["UI:Colors:StatusBarBackgroundColor"] ?? "DarkSlateGray";
    }

    public Style GetNormalStyle()
    {
        return new Style(foreground: GetColorByName(TextColor), background: GetColorByName(BackgroundColor));
    }

    public Style GetHighlightStyle()
    {
        return new Style(foreground: GetColorByName(HighlightTextColor), background: GetColorByName(HighlightBackgroundColor));
    }

    public Style GetErrorStyle()
    {
        return new Style(foreground: GetColorByName(ErrorColor), background: GetColorByName(BackgroundColor));
    }

    public Style GetStatusBarStyle()
    {
        return new Style(foreground: GetColorByName(TextColor), background: GetColorByName(StatusBarBackgroundColor));
    }

    private Color GetColorByName(string colorName)
    {
        return colorName?.ToLowerInvariant() switch
        {
            "black" => Color.Black,
            "white" => Color.White,
            "red" => Color.Red,
            "green" => Color.Green,
            "blue" => Color.Blue,
            "yellow" => Color.Yellow,
            "grey23" => Color.Grey23,
            "darkslategray" => Color.Teal,
            _ => Color.White
        };
    }
}
