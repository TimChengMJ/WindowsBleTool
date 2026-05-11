namespace BleTool.Shared.Logging;

public enum LogCategory
{
    Scan,
    Connect,
    Gatt,
    Read,
    Write,
    Subscribe,
    Notify,
    Error
}

public class LogEntry
{
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.Now;
    public LogCategory Category { get; init; }
    public string Message { get; init; } = string.Empty;

    public override string ToString() =>
        $"[{Timestamp:HH:mm:ss.fff}] [{Category}] {Message}";
}
