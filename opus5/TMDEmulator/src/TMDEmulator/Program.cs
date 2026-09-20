using System.Diagnostics.CodeAnalysis;
using Spectre.Console;
using TMDEmulator.Config;
using TMDEmulator.Crypto;
using TMDEmulator.Screens;
using TMDEmulator.Tui;

namespace TMDEmulator;

[ExcludeFromCodeCoverage(Justification = "Entry point wiring only; verified by the interactive smoke test.")]
internal static class Program
{
    private static int Main()
    {
        if (Console.IsInputRedirected || Console.IsOutputRedirected)
        {
            Console.Error.WriteLine("TMD Emulator is an interactive program and must be run in a terminal.");
            return 1;
        }

        (Theme theme, string? configWarning) = ConfigLoader.LoadTheme(Path.Combine(AppContext.BaseDirectory, ConfigLoader.FileName));
        StatusMessage? initialMessage = configWarning is null ? null : StatusMessage.Error(configWarning);

        var terminal = new SystemTerminal(AnsiConsole.Console, theme);
        MenuScreen root = MainMenu.Create(new MzmkInitExporter(TimeProvider.System), Directory.GetCurrentDirectory);
        terminal.Run(() => new App(terminal, root, initialMessage).Run());
        return 0;
    }
}
