using TMDEmulator.Terminal;

namespace TMDEmulator.Ui;

/// <summary>
/// Owns the screen stack, the two-frame layout and the key loop.
///
/// Layout (top to bottom): a Main frame (breadcrumb + the current screen) and a Bottom status frame
/// holding the key legend and, under it, up to <see cref="MessageLines"/> message lines (validation
/// errors / errors in red; long messages wrap, and are cut with an ellipsis only beyond that).
/// The rightmost terminal column is left unused: writing to the very last cell of the window makes
/// consoles scroll.
/// </summary>
public sealed class ScreenHost
{
    public const int MinWidth = 60;
    public const int MinHeight = 22;

    private const int MessageLines = 2;
    private const int StatusHeight = 3 + MessageLines; // top border, legend, messages, bottom border
    private const int PollMilliseconds = 100;

    private readonly ITerminal _terminal;
    private readonly Theme _theme;
    private readonly Stack<IScreen> _screens = new();

    private ScreenBuffer? _shown;
    private bool _exitRequested;
    private string _message = string.Empty;
    private bool _messageIsError;

    public ScreenHost(ITerminal terminal, Theme theme, IScreen root, IReadOnlyList<string> startupErrors)
    {
        _terminal = terminal;
        _theme = theme;
        _screens.Push(root);
        if (startupErrors.Count > 0)
            ShowError(string.Join("; ", startupErrors));
    }

    public IScreen Current => _screens.Peek();
    public int Depth => _screens.Count;
    public bool ExitRequested => _exitRequested;

    public void Push(IScreen screen) => _screens.Push(screen);

    /// <summary>Back to the previous screen. Does nothing on the top-level screen.</summary>
    public void Pop()
    {
        if (_screens.Count > 1)
            _screens.Pop();
    }

    public void Exit() => _exitRequested = true;

    public void ShowError(string message) => SetMessage(message, isError: true);

    public void ShowInfo(string message) => SetMessage(message, isError: false);

    private void SetMessage(string message, bool isError)
    {
        _message = message;
        _messageIsError = isError;
    }

    /// <summary>Run until the user exits. The terminal is always restored, even on an exception.</summary>
    public void Run()
    {
        _terminal.Initialize();
        try
        {
            Redraw();
            while (!_exitRequested)
            {
                if (!_terminal.WaitForKey(PollMilliseconds))
                {
                    if (_shown is not null && (UsableWidth != _shown.Width || _terminal.Height != _shown.Height))
                        Redraw();
                    continue;
                }

                HandleKey(_terminal.ReadKey());
                if (!_exitRequested)
                    Redraw();
            }
        }
        finally
        {
            _terminal.Restore();
        }
    }

    public void HandleKey(ConsoleKeyInfo key)
    {
        // Ctrl+C is delivered as an ordinary key (see ITerminal.Initialize); treat it as "quit" everywhere.
        if (key.Key == ConsoleKey.C && key.Modifiers.HasFlag(ConsoleModifiers.Control))
        {
            Exit();
            return;
        }

        if (TooSmall)
            return;

        _message = string.Empty;
        Current.HandleKey(key, this);
    }

    public void Redraw()
    {
        var width = UsableWidth;
        var height = _terminal.Height;
        var buffer = new ScreenBuffer(width, height, _theme.Text, _theme.Background);
        var canvas = new Canvas(buffer, _theme, 0, 0, width, height);

        if (TooSmall)
            canvas.Write(0, 0, TextUtil.Fit($"Terminal too small: need at least {MinWidth}x{MinHeight}", width), Style.Error);
        else
            DrawLayout(canvas, width, height);

        var repaint = _shown is null || _shown.Width != width || _shown.Height != height;
        if (repaint)
            _terminal.Clear(_theme.Background);
        buffer.Flush(_terminal, repaint ? null : _shown);
        _shown = buffer;
    }

    private int UsableWidth => Math.Max(0, _terminal.Width - 1);

    private bool TooSmall => _terminal.Width < MinWidth || _terminal.Height < MinHeight;

    private void DrawLayout(Canvas canvas, int width, int height)
    {
        var mainHeight = height - StatusHeight;
        var innerWidth = width - 4;

        DrawBox(canvas, 0, width, mainHeight, "TMD Emulator");
        DrawBox(canvas, mainHeight, width, StatusHeight, null);

        var breadcrumb = string.Join(" › ", _screens.Reverse().Select(s => s.Title));
        canvas.Write(1, 2, TextUtil.Fit(breadcrumb, innerWidth));

        Current.Render(canvas.Sub(top1: 3, left1: 2, width1: innerWidth, height1: mainHeight - 4));

        canvas.Write(mainHeight + 1, 2, TextUtil.Fit(Current.Legend, innerWidth));

        var messageStyle = _messageIsError ? Style.Error : Style.Normal;
        for (var i = 0; i < MessageLines; i++)
        {
            var offset = i * innerWidth;
            if (offset >= _message.Length)
                break;

            // The last line takes whatever is left, so an over-long message ends with an ellipsis.
            var rest = _message[offset..];
            var line = i == MessageLines - 1 ? TextUtil.Fit(rest, innerWidth) : rest[..Math.Min(innerWidth, rest.Length)];
            canvas.Write(mainHeight + 2 + i, 2, line, messageStyle);
        }
    }

    private static void DrawBox(Canvas canvas, int top, int width, int height, string? title)
    {
        var topLine = new string('─', width - 2);
        if (title is not null)
        {
            var label = $" {title} ";
            topLine = "─" + label + new string('─', width - 3 - label.Length);
        }

        canvas.Write(top, 0, "┌" + topLine + "┐");
        for (var r = top + 1; r < top + height - 1; r++)
        {
            canvas.Write(r, 0, "│");
            canvas.Write(r, width - 1, "│");
        }

        canvas.Write(top + height - 1, 0, "└" + new string('─', width - 2) + "┘");
    }
}
