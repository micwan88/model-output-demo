using Microsoft.Extensions.Configuration;
using Spectre.Console;
using System.Text;

namespace TMDEmulator.Main.UI.Terminal;

public class TerminalRenderer
{
    private readonly ColorScheme _colorScheme;
    private readonly IConfiguration _config;

    public TerminalRenderer(ColorScheme colorScheme, IConfiguration config)
    {
        _colorScheme = colorScheme;
        _config = config;
    }

    public void RenderMainFrame(string content, string statusText = "")
    {
        Console.Clear();

        var (width, height) = TerminalInput.GetTerminalSize();
        var statusBarHeight = int.Parse(_config["UI:Display:StatusBarHeight"] ?? "2");
        var contentHeight = height - statusBarHeight - 1;

        RenderContent(content, contentHeight);
        RenderStatusBar(statusText, statusBarHeight);
    }

    private void RenderContent(string content, int height)
    {
        var panel = new Panel(content)
        {
            Border = BoxBorder.Rounded
        };

        AnsiConsole.Write(panel);
    }

    private void RenderStatusBar(string statusText, int height)
    {
        var panel = new Panel(statusText)
        {
            Border = BoxBorder.Rounded
        };

        AnsiConsole.Write(panel);
    }

    public void RenderMenu(string title, List<(string Label, bool IsSelected)> items, string statusText = "")
    {
        var menuContent = new StringBuilder();
        menuContent.AppendLine($"[bold]{title}[/]");
        menuContent.AppendLine();

        foreach (var (label, isSelected) in items)
        {
            if (isSelected)
            {
                menuContent.AppendLine($"[bold black on grey23]{label}[/]");
            }
            else
            {
                menuContent.AppendLine(label);
            }
        }

        RenderMainFrame(menuContent.ToString(), statusText);
    }

    public void RenderError(string errorMessage, string statusText = "")
    {
        var errorContent = $"[red]{errorMessage}[/]";
        RenderMainFrame(errorContent, statusText);
    }

    public void RenderKeyDetails(string curveType, string privateKeyHex, string publicKeyHex)
    {
        var content = new StringBuilder();
        content.AppendLine("[bold]Key Details[/]");
        content.AppendLine();
        content.AppendLine($"Curve Type: [yellow]{curveType}[/]");
        content.AppendLine();
        content.AppendLine("[bold]Private Key (HEX Scalar):[/]");
        content.AppendLine(WrapText(privateKeyHex, 60));
        content.AppendLine();
        content.AppendLine("[bold]Public Key (HEX):[/]");
        content.AppendLine(WrapText(publicKeyHex, 60));

        RenderMainFrame(content.ToString());
    }

    private string WrapText(string text, int lineLength)
    {
        var lines = new List<string>();
        for (int i = 0; i < text.Length; i += lineLength)
        {
            lines.Add(text.Substring(i, Math.Min(lineLength, text.Length - i)));
        }

        return string.Join("\n", lines);
    }
}
