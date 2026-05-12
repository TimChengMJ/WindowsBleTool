namespace BleCore;

public class ScanResultEvent : EventArgs
{
    public BleDeviceInfo Device { get; init; } = null!;
}

public class ConnectionStateEvent : EventArgs
{
    public string DeviceId { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public bool IsConnected { get; init; }
}

public class NotificationEvent : EventArgs
{
    public string DeviceId { get; init; } = string.Empty;
    public string ServiceUuid { get; init; } = string.Empty;
    public string CharacteristicUuid { get; init; } = string.Empty;
    public byte[] Data { get; init; } = Array.Empty<byte>();
}
