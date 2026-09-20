namespace TMDEmulator.Tui;

internal enum MessageKind
{
    Error,
    Success,
}

/// <summary>Message shown on the status bar under the legend until the next key press.</summary>
internal sealed record StatusMessage(string Text, MessageKind Kind)
{
    public static StatusMessage Error(string text) => new(text, MessageKind.Error);

    public static StatusMessage Success(string text) => new(text, MessageKind.Success);
}

internal enum NavAction
{
    Stay,
    Push,
    Pop,
    Exit,
}

/// <summary>What the app should do after a screen handled a key.</summary>
internal sealed record ScreenResult(NavAction Action, IScreen? Next = null, StatusMessage? Message = null)
{
    public static ScreenResult Stay(StatusMessage? message = null) => new(NavAction.Stay, Message: message);

    public static ScreenResult Push(IScreen next) => new(NavAction.Push, next);

    public static ScreenResult Pop(StatusMessage? message = null) => new(NavAction.Pop, Message: message);

    public static ScreenResult Exit { get; } = new(NavAction.Exit);
}
