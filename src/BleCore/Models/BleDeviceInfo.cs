namespace BleCore.Models;

public class BleDeviceInfo
{
    public string DeviceId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public ulong BluetoothAddress { get; init; }
    public string AddressString =>
        $"{(BluetoothAddress >> 40) & 0xFF:X2}:" +
        $"{(BluetoothAddress >> 32) & 0xFF:X2}:" +
        $"{(BluetoothAddress >> 24) & 0xFF:X2}:" +
        $"{(BluetoothAddress >> 16) & 0xFF:X2}:" +
        $"{(BluetoothAddress >> 8) & 0xFF:X2}:" +
        $"{(BluetoothAddress) & 0xFF:X2}";
    public short Rssi { get; init; }
    public AdvertisementData? AdvertisementData { get; init; }
    public bool IsConnected { get; init; }
    public DateTimeOffset LastSeen { get; init; }
}
