using TMDEmulator.Config;
using TMDEmulator.Export;
using TMDEmulator.Keys;
using TMDEmulator.Terminal;
using TMDEmulator.Ui;

if (Console.IsInputRedirected || Console.IsOutputRedirected)
{
    Console.Error.WriteLine("TMDEmulator needs an interactive terminal (input/output must not be redirected).");
    return 1;
}

var config = ConfigLoader.Load(Path.Combine(AppContext.BaseDirectory, "tmdemulator.json"));
var exporter = new MzmkInitExporter(EcKeyInspector.Inspect(TestKeyPair.Pkcs8Pem), TimeProvider.System);
var root = MenuDefinition.CreateRoot(exporter, Directory.GetCurrentDirectory);

new ScreenHost(new ConsoleTerminal(), config.Theme, root, config.Errors).Run();
return 0;
