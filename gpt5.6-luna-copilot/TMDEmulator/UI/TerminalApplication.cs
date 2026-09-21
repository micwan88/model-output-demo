using TMDEmulator.Configuration;
using TMDEmulator.Security;

namespace TMDEmulator.UI;

public sealed class TerminalApplication
{
    private readonly TerminalRenderer _renderer;
    private readonly MzmkInitializationExporter _exporter;
    private readonly MenuNavigator _navigator;
    private string? _statusMessage;
    private bool _statusIsError;

    public TerminalApplication(
        TerminalOptions options,
        MzmkInitializationExporter exporter)
    {
        _renderer = new TerminalRenderer(options);
        _exporter = exporter;
        _navigator = new MenuNavigator(MenuDefinitions.CreateRoot());
    }

    public int Run()
    {
        var previousCursorVisible = true;
        if (OperatingSystem.IsWindows())
        {
            previousCursorVisible = Console.CursorVisible;
            Console.CursorVisible = false;
        }

        try
        {
            while (true)
            {
                _renderer.RenderMenu(
                    _navigator.CurrentScreen,
                    _navigator.SelectedIndex,
                    _statusMessage,
                    _statusIsError);
                _statusMessage = null;
                _statusIsError = false;

                var key = Console.ReadKey(intercept: true).Key;
                switch (key)
                {
                    case ConsoleKey.UpArrow:
                        _navigator.MoveSelection(-1);
                        break;
                    case ConsoleKey.DownArrow:
                        _navigator.MoveSelection(1);
                        break;
                    case ConsoleKey.Escape:
                        _navigator.GoBack();
                        break;
                    case ConsoleKey.Enter:
                        if (HandleSelection(_navigator.Activate()))
                        {
                            return 0;
                        }

                        break;
                }
            }
        }
        finally
        {
            Console.ResetColor();
            if (OperatingSystem.IsWindows())
            {
                Console.CursorVisible = previousCursorVisible;
            }
        }
    }

    private bool HandleSelection(MenuItem item)
    {
        switch (item.Action)
        {
            case MenuAction.Exit:
                return true;
            case MenuAction.Back:
                _navigator.GoBack();
                return false;
            case MenuAction.ExportInitializationKey:
                RunExport();
                return false;
            case MenuAction.Placeholder:
                ShowPlaceholder(item.Label);
                return false;
            case MenuAction.None:
                PushChildMenu(item.Label);
                return false;
            default:
                throw new InvalidOperationException($"Unsupported menu action: {item.Action}");
        }
    }

    private void PushChildMenu(string label)
    {
        var screen = label switch
        {
            "Remote MZMK Setup" => MenuDefinitions.CreateRemoteMzmkSetup(),
            "Key Management" => MenuDefinitions.CreateKeyManagement(),
            _ => throw new InvalidOperationException($"Unsupported menu: {label}")
        };
        _navigator.Push(screen);
    }

    private void ShowPlaceholder(string title)
    {
        _renderer.RenderPlaceholder(title, "This page is reserved for a later sub-story.");
        while (Console.ReadKey(intercept: true).Key != ConsoleKey.Escape)
        {
        }
    }

    private void RunExport()
    {
        _renderer.RenderKeyDetails(_exporter.Details, null, statusIsError: false);
        Console.WriteLine();
        Console.Write($"Output directory [{Environment.CurrentDirectory}]: ");
        var input = Console.ReadLine();
        var outputDirectory = string.IsNullOrWhiteSpace(input)
            ? Environment.CurrentDirectory
            : input.Trim();

        try
        {
            var timestamp = DateTime.Now;
            var targetPath = _exporter.BuildTargetPath(outputDirectory, timestamp);
            var overwrite = false;

            if (File.Exists(targetPath))
            {
                Console.Write($"File exists. Overwrite '{targetPath}'? [y/N]: ");
                overwrite = IsYes(Console.ReadLine());
                if (!overwrite)
                {
                    _statusMessage = "Export cancelled; the existing file was not changed.";
                    return;
                }
            }

            var savedPath = _exporter.Export(outputDirectory, timestamp, overwrite);
            _statusMessage = $"Initialization key saved: {savedPath}";
        }
        catch (ArgumentException exception)
        {
            _statusMessage = exception.Message;
            _statusIsError = true;
        }
        catch (DirectoryNotFoundException exception)
        {
            _statusMessage = exception.Message;
            _statusIsError = true;
        }
        catch (IOException exception)
        {
            _statusMessage = exception.Message;
            _statusIsError = true;
        }
        catch (UnauthorizedAccessException exception)
        {
            _statusMessage = exception.Message;
            _statusIsError = true;
        }
    }

    private static bool IsYes(string? input)
    {
        return string.Equals(input?.Trim(), "y", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(input?.Trim(), "yes", StringComparison.OrdinalIgnoreCase);
    }
}
