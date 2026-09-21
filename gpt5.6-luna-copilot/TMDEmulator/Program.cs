using TMDEmulator.Configuration;
using TMDEmulator.Security;
using TMDEmulator.UI;

namespace TMDEmulator;

internal static class Program
{
    private static int Main()
    {
        if (Console.IsInputRedirected || Console.IsOutputRedirected)
        {
            Console.Error.WriteLine("TMD Emulator requires an interactive terminal.");
            return 1;
        }

        try
        {
            var options = TerminalOptionsLoader.Load(
                Path.Combine(AppContext.BaseDirectory, "appsettings.json"));
            var exporter = new MzmkInitializationExporter(TestKeyMaterial.Pkcs8Pem);
            return new TerminalApplication(options, exporter).Run();
        }
        catch (ConfigurationException exception)
        {
            Console.Error.WriteLine($"Configuration error: {exception.Message}");
            return 1;
        }
        catch (System.Security.Cryptography.CryptographicException exception)
        {
            Console.Error.WriteLine($"Key material error: {exception.Message}");
            return 1;
        }
    }
}
