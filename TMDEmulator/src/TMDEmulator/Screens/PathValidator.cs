namespace TMDEmulator.Screens;

internal sealed record PathValidationResult(string? FullPath, string? Error)
{
    public bool IsValid => Error is null;
}

/// <summary>Validates the user-entered output directory.</summary>
internal static class PathValidator
{
    // Rejected on every platform so behaviour is identical on Windows and Linux.
    private static readonly char[] InvalidChars = { '<', '>', '"', '|', '?', '*' };

    /// <param name="input">Directory entered by the user; relative paths resolve against <paramref name="currentDirectory"/>.</param>
    public static PathValidationResult ValidateDirectory(string input, string currentDirectory)
    {
        string path = input.Trim();
        if (path.Length == 0)
        {
            return Fail("Output path is required.");
        }

        if (path.Any(c => char.IsControl(c) || InvalidChars.Contains(c)))
        {
            return Fail($"Output path contains an invalid character (not allowed: {string.Join(" ", InvalidChars)} or control characters).");
        }

        string fullPath;
        try
        {
            fullPath = Path.GetFullPath(path, currentDirectory);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return Fail($"Output path is invalid: {ex.Message}");
        }

        if (File.Exists(fullPath))
        {
            return Fail($"Output path is a file, not a directory: {fullPath}");
        }

        if (!Directory.Exists(fullPath))
        {
            return Fail($"Directory does not exist: {fullPath}");
        }

        return new PathValidationResult(fullPath, null);
    }

    private static PathValidationResult Fail(string error) => new(null, error);
}
