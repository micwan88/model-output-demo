using TMDEmulator.Export;

namespace TMDEmulator.Ui;

/// <summary>
/// Export MZMK Initialization Key: shows the hardcoded keypair, asks for the output folder (default: the
/// current directory), then saves the initialization file. If the file already exists the user is asked
/// whether to overwrite it. Problems are reported in red in the status bar; the prompt stays open.
/// </summary>
public sealed class ExportInitKeyScreen : IScreen
{
    private const string InputLegend = "←/→ Move   Enter Save   Esc Back";
    private const string ConfirmLegend = "↑/↓ Navigate   Enter Select   Esc No";

    private readonly MzmkInitExporter _exporter;
    private readonly LineEditor _editor;
    private readonly SelectionList _overwriteChoice = new(["Yes", "No"]);
    private ExportTarget? _pendingOverwrite;

    public ExportInitKeyScreen(MzmkInitExporter exporter, string defaultFolder)
    {
        _exporter = exporter;
        _editor = new LineEditor(defaultFolder);
    }

    public string Title => "Export MZMK Initialization Key";

    public string Legend => _pendingOverwrite is null ? InputLegend : ConfirmLegend;

    public bool AwaitingOverwriteAnswer => _pendingOverwrite is not null;

    public void Render(Canvas canvas)
    {
        var key = _exporter.Key;
        var row = 0;
        var curve = key.CurveOid.Length > 0 ? $"{key.CurveName} (OID {key.CurveOid})" : key.CurveName;

        canvas.Write(row++, 0, $"Curve type: {curve}");
        canvas.Write(row++, 0, "Private key (HEX scalar):");
        row = WriteWrapped(canvas, row, key.PrivateKeyHex);
        row++;
        canvas.Write(row++, 0, "Public key (HEX, 04 || X || Y):");
        row = WriteWrapped(canvas, row, key.PublicKeyHex);
        row++;

        if (_pendingOverwrite is null)
        {
            canvas.Write(row++, 0, "Output folder for the MZMK Initialization file:");
            _editor.Render(canvas, row);
        }
        else
        {
            canvas.Write(row++, 0, TextUtil.Fit($"File already exists: {_pendingOverwrite.FullPath}", canvas.Width));
            canvas.Write(row++, 0, "Overwrite it?");
            _overwriteChoice.Render(canvas, row);
        }
    }

    public void HandleKey(ConsoleKeyInfo key, ScreenHost host)
    {
        if (_pendingOverwrite is not null)
            HandleOverwriteAnswer(key, host);
        else
            HandleInput(key, host);
    }

    private void HandleInput(ConsoleKeyInfo key, ScreenHost host)
    {
        switch (key.Key)
        {
            case ConsoleKey.Escape:
                host.Pop();
                return;
            case ConsoleKey.Enter:
                Submit(host);
                return;
            default:
                _editor.HandleKey(key);
                return;
        }
    }

    private void HandleOverwriteAnswer(ConsoleKeyInfo key, ScreenHost host)
    {
        if (_overwriteChoice.HandleKey(key))
            return;

        switch (key.Key)
        {
            case ConsoleKey.Escape:
                _pendingOverwrite = null;
                return;
            case ConsoleKey.Enter:
                var target = _pendingOverwrite!;
                var yes = _overwriteChoice.Index == 0;
                _pendingOverwrite = null;
                if (yes)
                    Save(target, overwrite: true, host);
                return;
        }
    }

    private void Submit(ScreenHost host)
    {
        if (!_exporter.TryPlan(_editor.Text, out var target, out var error))
        {
            host.ShowError(error);
            return;
        }

        if (target.Exists)
        {
            _overwriteChoice.Reset();
            _pendingOverwrite = target;
            return;
        }

        Save(target, overwrite: false, host);
    }

    private void Save(ExportTarget target, bool overwrite, ScreenHost host)
    {
        try
        {
            _exporter.Write(target.FullPath, overwrite);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            host.ShowError($"Cannot write {target.FullPath}: {e.Message}");
            return;
        }

        host.Pop();
        host.ShowInfo($"Saved: {target.FullPath}");
    }

    private static int WriteWrapped(Canvas canvas, int row, string text)
    {
        foreach (var line in TextUtil.Wrap(text, canvas.Width - 2))
            canvas.Write(row++, 2, line);
        return row;
    }
}
