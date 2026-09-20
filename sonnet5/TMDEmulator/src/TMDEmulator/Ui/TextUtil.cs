namespace TMDEmulator.Ui;

public static class TextUtil
{
    /// <summary>Split text into fixed-width chunks (for long HEX strings that have no spaces).</summary>
    public static IEnumerable<string> Wrap(string text, int width)
    {
        if (width <= 0)
            yield break;

        for (var i = 0; i < text.Length; i += width)
            yield return text.Substring(i, Math.Min(width, text.Length - i));
    }

    /// <summary>Clip text to <paramref name="width"/>, ending with an ellipsis when something was cut.</summary>
    public static string Fit(string text, int width)
    {
        if (width <= 0)
            return string.Empty;
        if (text.Length <= width)
            return text;
        return width == 1 ? "…" : text[..(width - 1)] + "…";
    }
}
