namespace TMDEmulator.Main.Utilities;

public static class ValidationHelper
{
    public static bool IsValidDirectoryPath(string path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            var expandedPath = Path.GetFullPath(path);
            return Directory.Exists(expandedPath);
        }
        catch
        {
            return false;
        }
    }

    public static bool IsDirectoryWritable(string path)
    {
        try
        {
            if (!Directory.Exists(path))
                return false;

            var testFile = Path.Combine(path, $".tmd_write_test_{Guid.NewGuid()}");
            File.WriteAllText(testFile, "test");
            File.Delete(testFile);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static (bool IsValid, string ErrorMessage) ValidateOutputPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return (false, "Path cannot be empty");
        }

        if (!IsValidDirectoryPath(path))
        {
            return (false, "Directory does not exist");
        }

        if (!IsDirectoryWritable(path))
        {
            return (false, "Directory is not writable");
        }

        return (true, string.Empty);
    }

    public static string SanitizeFilePath(string path)
    {
        try
        {
            return Path.GetFullPath(path);
        }
        catch
        {
            return path;
        }
    }

    public static string ExpandPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return Directory.GetCurrentDirectory();

        if (path == "./" || path == ".")
            return Directory.GetCurrentDirectory();

        try
        {
            return Path.GetFullPath(path);
        }
        catch
        {
            return path;
        }
    }
}
