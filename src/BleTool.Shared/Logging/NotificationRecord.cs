namespace BleTool.Shared.Logging;

public class NotificationRecord
{
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.Now;
    public string DeviceName { get; init; } = string.Empty;
    public string CharacteristicUuid { get; init; } = string.Empty;
    public string HexValue { get; init; } = string.Empty;
    public string ParsedValue { get; init; } = string.Empty;
}
