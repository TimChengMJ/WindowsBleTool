namespace BleCore;

public class BleDeviceInfo
{
    public string DeviceId { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int Rssi { get; init; }
    public string AddressType { get; init; } = "Public";
    public bool IsConnected { get; set; }
    public DateTimeOffset LastSeen { get; set; }
    public List<BleServiceInfo> Services { get; init; } = new();
    public byte[] RawAdvertisement { get; init; } = Array.Empty<byte>();
    public Dictionary<ushort, byte[]> AdStructures { get; init; } = new();
}
