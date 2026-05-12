namespace BleCore;

public class BleCharacteristicInfo
{
    public string Uuid { get; init; } = string.Empty;
    public string DisplayName => GattKnownUuids.GetName(Uuid);
    public string ServiceUuid { get; init; } = string.Empty;
    public GattPermissions Permissions { get; init; }
    public byte[] LastValue { get; set; } = Array.Empty<byte>();
    public string LastValueHex => LastValue.Length > 0
        ? BitConverter.ToString(LastValue).Replace("-", " ")
        : "";
}

[Flags]
public enum GattPermissions
{
    None = 0,
    Read = 1,
    Write = 2,
    WriteWithoutResponse = 4,
    Notify = 8,
    Indicate = 16,
}
