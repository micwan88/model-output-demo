using TMDEmulator.Crypto;
using TMDEmulator.Tui;

namespace TMDEmulator.Screens;

/// <summary>
/// Shows the MZMK Initialization keypair details, asks for an output directory, and saves
/// the SPKI HEX file (confirming before overwriting an existing file).
/// </summary>
internal sealed class ExportMzmkInitScreen : IScreen
{
    private readonly EcKeyInfo _key;
    private readonly MzmkInitExporter _exporter;
    private readonly string _currentDirectory;
    private readonly TextInput _input;
    private ConfirmPrompt? _confirm;
    private string? _pendingFilePath;

    public ExportMzmkInitScreen(EcKeyInfo key, MzmkInitExporter exporter, string currentDirectory)
    {
        _key = key;
        _exporter = exporter;
        _currentDirectory = currentDirectory;
        _input = new TextInput(currentDirectory);
    }

    public string Title => "Export MZMK Initialization Key";

    public string Legend => _confirm is null
        ? "Enter Save · Esc Back · ←/→ Home/End Move cursor"
        : "←/→ Choose · Enter Confirm · Esc Cancel";

    internal bool IsConfirmingOverwrite => _confirm is not null;

    internal TextInput Input => _input;

    public IReadOnlyList<BodyLine> Render()
    {
        var lines = new List<BodyLine>
        {
            BodyLine.Of("MZMK Initialization keypair (internal TEST key)", Role.Title),
            BodyLine.Empty,
            BodyLine.Of($"Curve type : {_key.CurveType}"),
            BodyLine.Empty,
            BodyLine.Of("Private key (HEX scalar):"),
            BodyLine.Of(_key.PrivateKeyHex),
            BodyLine.Empty,
            BodyLine.Of("Public key (HEX):"),
            BodyLine.Of(_key.PublicKeyHex),
            BodyLine.Empty,
            BodyLine.Of("Output directory for the MZMK Initialization file:"),
            _input.Render(),
        };

        if (_confirm is not null)
        {
            lines.Add(BodyLine.Empty);
            lines.Add(BodyLine.Of($"File already exists: {Path.GetFileName(_pendingFilePath)}. Overwrite?"));
            lines.Add(_confirm.Render());
        }

        return lines;
    }

    public ScreenResult HandleKey(ConsoleKeyInfo key)
    {
        if (_confirm is not null)
        {
            switch (_confirm.HandleKey(key))
            {
                case ConfirmOutcome.Yes:
                    return Save(_pendingFilePath!, overwrite: true);
                case ConfirmOutcome.No:
                    _confirm = null;
                    _pendingFilePath = null;
                    break;
            }

            return ScreenResult.Stay();
        }

        switch (_input.HandleKey(key))
        {
            case InputOutcome.Cancelled:
                return ScreenResult.Pop();
            case InputOutcome.Submitted:
                return Submit();
            default:
                return ScreenResult.Stay();
        }
    }

    private ScreenResult Submit()
    {
        PathValidationResult validation = PathValidator.ValidateDirectory(_input.Value, _currentDirectory);
        if (!validation.IsValid)
        {
            return ScreenResult.Stay(StatusMessage.Error(validation.Error!));
        }

        string filePath = Path.Combine(validation.FullPath!, _exporter.BuildFileName(_key));
        if (File.Exists(filePath))
        {
            _confirm = new ConfirmPrompt();
            _pendingFilePath = filePath;
            return ScreenResult.Stay();
        }

        return Save(filePath, overwrite: false);
    }

    private ScreenResult Save(string filePath, bool overwrite)
    {
        _confirm = null;
        _pendingFilePath = null;
        try
        {
            MzmkInitExporter.Write(filePath, _key, overwrite);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return ScreenResult.Stay(StatusMessage.Error($"Failed to save file: {ex.Message}"));
        }

        return ScreenResult.Pop(StatusMessage.Success($"Saved: {filePath}"));
    }
}
