using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TMDEmulator.Main.UI.Screens;
using TMDEmulator.Main.UI.Terminal;

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .Build();

var services = new ServiceCollection()
    .AddSingleton<IConfiguration>(config)
    .AddSingleton<ColorScheme>()
    .AddSingleton<TerminalRenderer>()
    .BuildServiceProvider();

var renderer = services.GetRequiredService<TerminalRenderer>();
var colorScheme = services.GetRequiredService<ColorScheme>();

BaseScreen? currentScreen = new MainMenuScreen(renderer, colorScheme);

while (currentScreen != null)
{
    currentScreen.Render();

    var keyInfo = TerminalInput.ReadKey();
    var nextScreen = currentScreen.HandleInput(keyInfo);

    if (nextScreen == null && currentScreen is MainMenuScreen)
    {
        break;
    }

    currentScreen = nextScreen ?? currentScreen;
}

Console.Clear();
Console.WriteLine("Thank you for using TMD Emulator!");
