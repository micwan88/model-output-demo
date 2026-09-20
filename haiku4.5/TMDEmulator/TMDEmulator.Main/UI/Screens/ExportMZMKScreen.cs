using System.Security.Cryptography;
using System.Text;
using TMDEmulator.Main.Crypto;
using TMDEmulator.Main.Models;
using TMDEmulator.Main.UI.Terminal;
using TMDEmulator.Main.Utilities;
using Spectre.Console;

namespace TMDEmulator.Main.UI.Screens;

public class ExportMZMKScreen : BaseScreen
{
    private enum State { DisplayingKeyDetails, PromptingPath, ProcessingExport, Success, Failed }

    private State _currentState = State.DisplayingKeyDetails;
    private ECKeyManager _keyManager;
    private string _selectedPath = "";
    private ExportResult? _exportResult;

    public ExportMZMKScreen(TerminalRenderer renderer, ColorScheme colorScheme, BaseScreen parentScreen)
        : base(renderer, colorScheme, parentScreen)
    {
        _keyManager = new ECKeyManager();
        _keyManager.LoadPrivateKey(KeyConstants.TestingECPrivateKeyPem);
    }

    public override void Render()
    {
        switch (_currentState)
        {
            case State.DisplayingKeyDetails:
                RenderKeyDetails();
                break;
            case State.PromptingPath:
                RenderPathPrompt();
                break;
            case State.ProcessingExport:
                RenderProcessing();
                break;
            case State.Success:
                RenderSuccess();
                break;
            case State.Failed:
                RenderFailed();
                break;
        }
    }

    private void RenderKeyDetails()
    {
        var curveType = _keyManager.GetCurveType();
        var privateKeyHex = _keyManager.GetPrivateKeyHex() ?? "Unable to extract";
        var publicKeyHex = _keyManager.GetPublicKeyHex() ?? "Unable to extract";

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
        content.AppendLine();
        content.AppendLine("[dim]Press Enter to proceed with export[/]");

        Renderer.RenderMainFrame(content.ToString(), GetStatusBarText());
    }

    private void RenderPathPrompt()
    {
        var defaultPath = Path.GetFullPath("./");
        var content = new StringBuilder();
        content.AppendLine("[bold]Export MZMK Initialization Key[/]");
        content.AppendLine();
        content.AppendLine("Enter the output directory path:");
        content.AppendLine($"[dim]Default: {defaultPath}[/]");
        content.AppendLine();
        content.AppendLine("Path: ");

        Renderer.RenderMainFrame(content.ToString(), GetStatusBarText());

        var userPath = TerminalInput.ReadLine();

        if (string.IsNullOrWhiteSpace(userPath))
        {
            _selectedPath = defaultPath;
        }
        else
        {
            _selectedPath = ValidationHelper.ExpandPath(userPath);
        }

        var (isValid, errorMessage) = ValidationHelper.ValidateOutputPath(_selectedPath);

        if (!isValid)
        {
            ErrorMessage = errorMessage;
            _currentState = State.Failed;
        }
        else
        {
            _currentState = State.ProcessingExport;
        }
    }

    private void RenderProcessing()
    {
        try
        {
            var ecKey = _keyManager.GetECDsa();
            if (ecKey == null)
            {
                throw new InvalidOperationException("EC key not loaded");
            }

            var spiDer = KeyExporter.GenerateSPKIDer(ecKey);
            var fingerprint = KeyExporter.ComputeFingerprint(spiDer);
            var filename = KeyExporter.GenerateFilename(fingerprint);
            var spiHex = Convert.ToHexString(spiDer);

            var success = KeyExporter.ExportToFile(_selectedPath, spiHex, filename);

            if (success)
            {
                _exportResult = new ExportResult(
                    true,
                    KeyExporter.GetExportedFilePath(_selectedPath, filename),
                    fingerprint
                );
                _currentState = State.Success;
            }
            else
            {
                _exportResult = new ExportResult(false, errorMessage: "Failed to write file");
                _currentState = State.Failed;
            }
        }
        catch (Exception ex)
        {
            _exportResult = new ExportResult(false, errorMessage: ex.Message);
            _currentState = State.Failed;
        }
    }

    private void RenderSuccess()
    {
        var content = new StringBuilder();
        content.AppendLine("[green]Export Successful![/]");
        content.AppendLine();
        content.AppendLine($"File: [yellow]{_exportResult?.FilePath}[/]");
        content.AppendLine($"Fingerprint: [yellow]{_exportResult?.Fingerprint}[/]");
        content.AppendLine();
        content.AppendLine("[dim]Press any key to return to menu[/]");

        Renderer.RenderMainFrame(content.ToString(), GetStatusBarText());
    }

    private void RenderFailed()
    {
        var content = new StringBuilder();
        content.AppendLine("[red]Export Failed[/]");
        content.AppendLine();
        content.AppendLine($"[red]Error: {ErrorMessage ?? _exportResult?.ErrorMessage}[/]");
        content.AppendLine();
        content.AppendLine("[dim]Press any key to return to menu[/]");

        Renderer.RenderMainFrame(content.ToString(), GetStatusBarText());
    }

    public override BaseScreen? HandleInput(ConsoleKeyInfo keyInfo)
    {
        switch (_currentState)
        {
            case State.DisplayingKeyDetails:
                if (TerminalInput.IsEnter(keyInfo.Key))
                {
                    _currentState = State.PromptingPath;
                    Render();
                }
                break;

            case State.PromptingPath:
                break;

            case State.Success:
            case State.Failed:
                if (TerminalInput.IsEscape(keyInfo.Key) || keyInfo.KeyChar != '\0')
                {
                    return ParentScreen;
                }
                break;
        }

        return this;
    }

    public override string GetStatusBarText()
    {
        return _currentState switch
        {
            State.DisplayingKeyDetails => "Enter to proceed | Esc to cancel",
            State.PromptingPath => "Enter path (empty for current directory)",
            State.Success => "Esc to return",
            State.Failed => "Esc to return",
            _ => ""
        };
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
