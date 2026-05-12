// src/BleCore/IBleAdapter.cs
namespace BleCore;

public class DeviceFilterRule
{
    public FilterType Type { get; init; }
    public FilterOperator Operator { get; init; }
    public string Value { get; init; } = string.Empty;
}

public enum FilterType { Rssi, DeviceName, AdvertisedUuid, MacAddress, ManufacturerId, RawData, AddressType }
public enum FilterOperator { Equal, GreaterThanOrEqual, LessThanOrEqual, Contains, NotContains, Regex }

public interface IBleAdapter
{
    event EventHandler<ScanResultEvent>? ScanResult;
    event EventHandler<ConnectionStateEvent>? ConnectionStateChanged;
    event EventHandler<NotificationEvent>? NotificationReceived;

    Task StartScanAsync(IReadOnlyList<DeviceFilterRule>? filters = null, CancellationToken ct = default);
    void StopScan();

    Task ConnectAsync(string deviceId);
    Task DisconnectAsync(string deviceId);
    Task<IReadOnlyList<BleServiceInfo>> DiscoverServicesAsync(string deviceId);
    Task<byte[]> ReadCharacteristicAsync(string deviceId, string serviceUuid, string characteristicUuid);
    Task WriteCharacteristicAsync(string deviceId, string serviceUuid, string characteristicUuid, byte[] data, bool withResponse = true);
    Task SubscribeCharacteristicAsync(string deviceId, string serviceUuid, string characteristicUuid);
    Task UnsubscribeCharacteristicAsync(string deviceId, string serviceUuid, string characteristicUuid);
}
