using IvkExportTool.Desktop.Enums;

namespace IvkExportTool.Desktop.Events;

public class NotificationRequestedEventArgs : EventArgs
{
    public string Message { get; }
    public StatusMessageType Type { get; }

    public NotificationRequestedEventArgs(string message, StatusMessageType type)
    {
        Message = message;
        Type = type;
    }
}
