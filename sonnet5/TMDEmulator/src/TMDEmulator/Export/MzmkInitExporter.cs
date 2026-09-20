using System.Globalization;
using System.Text;
using TMDEmulator.Keys;

namespace TMDEmulator.Export;

/// <summary>Where the initialization file would be written and whether a file is already there.</summary>
public sealed record ExportTarget(string FullPath, bool Exists);

/// <summary>
/// Produces the "MZMK Initialization" file: the uppercase HEX text of the public key point
/// (04 || X || Y). The file carries a <c>.der</c> extension although its content is HEX text; that is
/// how the real TMD names it.
/// </summary>
public sealed class MzmkInitExporter(EcKeyInfo key, TimeProvider clock)
{
    public EcKeyInfo Key { get; } = key;

    /// <summary>MZMK_Init_{8-char fingerprint}{yyyyMMddHHmmss}.der, timestamp in local time.</summary>
    public string BuildFileName()
    {
        var stamp = clock.GetLocalNow().ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
        return $"MZMK_Init_{Key.Fingerprint}{stamp}.der";
    }

    /// <summary>Validates the folder typed by the user and works out the target file.</summary>
    /// <returns>false with a user-facing <paramref name="error"/> when the folder is unusable.</returns>
    public bool TryPlan(string folderInput, out ExportTarget target, out string error)
    {
        target = new ExportTarget(string.Empty, false);
        error = string.Empty;

        var input = folderInput.Trim();
        if (input.Length == 0)
        {
            error = "Output folder must not be empty.";
            return false;
        }

        string folder;
        try
        {
            if (input.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
                throw new ArgumentException("illegal character");
            folder = Path.GetFullPath(input);
        }
        catch (Exception e) when (e is ArgumentException or NotSupportedException or PathTooLongException)
        {
            error = $"Invalid folder path: {input}";
            return false;
        }

        if (!Directory.Exists(folder))
        {
            error = $"Folder does not exist: {folder}";
            return false;
        }

        var fullPath = Path.Combine(folder, BuildFileName());
        target = new ExportTarget(fullPath, File.Exists(fullPath));
        return true;
    }

    /// <summary>Writes the HEX text (ASCII, no BOM, no trailing newline).</summary>
    /// <exception cref="IOException">The file exists and <paramref name="overwrite"/> is false, or it cannot be written.</exception>
    /// <exception cref="UnauthorizedAccessException">No permission to write there.</exception>
    public void Write(string fullPath, bool overwrite)
    {
        var mode = overwrite ? FileMode.Create : FileMode.CreateNew;
        using var stream = new FileStream(fullPath, mode, FileAccess.Write, FileShare.None);
        stream.Write(Encoding.ASCII.GetBytes(Key.PublicKeyHex));
    }
}
