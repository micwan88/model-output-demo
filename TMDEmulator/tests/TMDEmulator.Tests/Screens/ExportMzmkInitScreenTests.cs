using TMDEmulator.Crypto;
using TMDEmulator.Screens;
using TMDEmulator.Tests.Support;
using TMDEmulator.Tui;

namespace TMDEmulator.Tests.Screens;

public sealed class ExportMzmkInitScreenTests : IDisposable
{
    private static readonly string ExpectedFileName = $"MZMK_Init_{TestVectors.P256Fingerprint}20260919143005.der";

    private readonly TempDirectory _temp = new();
    private readonly ExportMzmkInitScreen _screen;

    public ExportMzmkInitScreenTests()
    {
        _screen = new ExportMzmkInitScreen(
            EcKeyInfo.FromPkcs8Pem(TestKeys.MzmkInitKeyPairPem),
            new MzmkInitExporter(FixedTimeProvider.HongKongAfternoon()),
            _temp.Path);
    }

    public void Dispose() => _temp.Dispose();

    private string ExpectedFilePath => _temp.Combine(ExpectedFileName);

    private void ReplaceInput(string text)
    {
        _screen.HandleKey(Keys.End);
        while (_screen.Input.Value.Length > 0)
        {
            _screen.HandleKey(Keys.Backspace);
        }

        foreach (ConsoleKeyInfo key in Keys.Text(text))
        {
            _screen.HandleKey(key);
        }
    }

    [Fact]
    public void Render_ShowsCurvePrivateAndPublicKey_AndPrefilledDirectory()
    {
        string[] text = _screen.Render().Select(l => l.PlainText).ToArray();

        Assert.Contains($"Curve type : NIST P-256 (secp256r1) - OID {TestVectors.P256Oid}", text);
        Assert.Contains("Private key (HEX scalar):", text);
        Assert.Contains(TestVectors.P256Private, text);
        Assert.Contains("Public key (HEX):", text);
        Assert.Contains(TestVectors.P256Public, text);
        Assert.Contains($"> {_temp.Path} ", text);
        Assert.Equal("Export MZMK Initialization Key", _screen.Title);
    }

    [Fact]
    public void Enter_WithDefaultDirectory_SavesFileAndGoesBackWithSuccess()
    {
        ScreenResult result = _screen.HandleKey(Keys.Enter);

        Assert.Equal(NavAction.Pop, result.Action);
        Assert.Equal(StatusMessage.Success($"Saved: {ExpectedFilePath}"), result.Message);
        Assert.Equal(TestVectors.P256Spki, File.ReadAllText(ExpectedFilePath));
    }

    [Fact]
    public void Enter_WithRelativeDirectory_SavesUnderCurrentDirectory()
    {
        Directory.CreateDirectory(_temp.Combine("out"));
        ReplaceInput("out");

        ScreenResult result = _screen.HandleKey(Keys.Enter);

        Assert.Equal(NavAction.Pop, result.Action);
        Assert.True(File.Exists(_temp.Combine("out", ExpectedFileName)));
    }

    [Fact]
    public void Escape_GoesBackWithoutSaving()
    {
        Assert.Equal(NavAction.Pop, _screen.HandleKey(Keys.Esc).Action);
        Assert.Empty(Directory.GetFiles(_temp.Path));
    }

    [Theory]
    [InlineData("", "Output path is required.")]
    [InlineData("missing", "Directory does not exist:")]
    [InlineData("a|b", "Output path contains an invalid character")]
    public void InvalidDirectory_StaysWithRedError(string input, string expectedError)
    {
        ReplaceInput(input);

        ScreenResult result = _screen.HandleKey(Keys.Enter);

        Assert.Equal(NavAction.Stay, result.Action);
        Assert.Equal(MessageKind.Error, result.Message!.Kind);
        Assert.StartsWith(expectedError, result.Message.Text);
    }

    [Fact]
    public void ExistingFile_AsksToOverwrite_NoKeepsFile()
    {
        File.WriteAllText(ExpectedFilePath, "original");

        ScreenResult ask = _screen.HandleKey(Keys.Enter);
        Assert.Equal(NavAction.Stay, ask.Action);
        Assert.True(_screen.IsConfirmingOverwrite);
        Assert.Contains($"File already exists: {ExpectedFileName}. Overwrite?", _screen.Render().Select(l => l.PlainText));
        Assert.Equal("←/→ Choose · Enter Confirm · Esc Cancel", _screen.Legend);

        ScreenResult no = _screen.HandleKey(Keys.Enter); // default selection is No

        Assert.Equal(NavAction.Stay, no.Action);
        Assert.False(_screen.IsConfirmingOverwrite);
        Assert.Equal("original", File.ReadAllText(ExpectedFilePath));
    }

    [Fact]
    public void ExistingFile_EscapeCancelsOverwrite()
    {
        File.WriteAllText(ExpectedFilePath, "original");
        _screen.HandleKey(Keys.Enter);

        _screen.HandleKey(Keys.Esc);

        Assert.False(_screen.IsConfirmingOverwrite);
        Assert.Equal("original", File.ReadAllText(ExpectedFilePath));
    }

    [Fact]
    public void ExistingFile_ArrowKeepsPrompt_YesOverwrites()
    {
        File.WriteAllText(ExpectedFilePath, "original");
        _screen.HandleKey(Keys.Enter);

        Assert.Equal(NavAction.Stay, _screen.HandleKey(Keys.Left).Action);
        Assert.True(_screen.IsConfirmingOverwrite);
        ScreenResult result = _screen.HandleKey(Keys.Enter);

        Assert.Equal(NavAction.Pop, result.Action);
        Assert.Equal(MessageKind.Success, result.Message!.Kind);
        Assert.Equal(TestVectors.P256Spki, File.ReadAllText(ExpectedFilePath));
    }

    [Fact]
    public void WriteFailure_StaysWithRedError()
    {
        // A directory occupying the target file name makes the write fail on every platform.
        Directory.CreateDirectory(ExpectedFilePath);
        File.WriteAllText(Path.Combine(ExpectedFilePath, "keep"), "x");

        _screen.HandleKey(Keys.Enter); // File.Exists is false for a directory, so no overwrite prompt
        ScreenResult result = _screen.HandleKey(Keys.Enter);

        Assert.Equal(NavAction.Stay, result.Action);
        Assert.Equal(MessageKind.Error, result.Message!.Kind);
        Assert.StartsWith("Failed to save file:", result.Message.Text);
    }

    [Fact]
    public void Legend_WhileEditing_DescribesKeys()
    {
        Assert.Equal("Enter Save · Esc Back · ←/→ Home/End Move cursor", _screen.Legend);
    }
}
