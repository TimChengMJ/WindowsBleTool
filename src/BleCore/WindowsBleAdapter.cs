// src/BleCore/WindowsBleAdapter.cs
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace BleCore;

public class WindowsBleAdapter : IBleAdapter, IDisposable
{
    private readonly BluetoothLEAdvertisementWatcher _watcher = new();
    private readonly Dictionary<string, BluetoothLEDevice> _connectedDevices = new();
    private readonly Dictionary<string, Dictionary<string, GattCharacteristic>> _subscribedChars = new();

    public event EventHandler<ScanResultEvent>? ScanResult;
    public event EventHandler<ConnectionStateEvent>? ConnectionStateChanged;
    public event EventHandler<NotificationEvent>? NotificationReceived;

    private IReadOnlyList<DeviceFilterRule>? _activeFilters;

    public WindowsBleAdapter()
    {
        _watcher.Received += OnAdvertisementReceived;
    }

    public Task StartScanAsync(IReadOnlyList<DeviceFilterRule>? filters = null, CancellationToken ct = default)
    {
        _activeFilters = filters;
        ct.Register(() => _watcher.Stop());
        _watcher.Start();
        return Task.CompletedTask;
    }

    public void StopScan() => _watcher.Stop();

    private void OnAdvertisementReceived(BluetoothLEAdvertisementWatcher sender, BluetoothLEAdvertisementReceivedEventArgs args)
    {
        var device = new BleDeviceInfo
        {
            DeviceId = args.BluetoothAddress.ToString("X12"),
            Address = ConvertToMacAddress(args.BluetoothAddress),
            Name = args.Advertisement.LocalName ?? "Unknown",
            Rssi = args.RawSignalStrengthInDBm,
            AddressType = args.BluetoothAddressType == BluetoothAddressType.Random ? "Random" : "Public",
            LastSeen = DateTimeOffset.UtcNow,
            RawAdvertisement = ToByteArray(args.RawSignalStrengthInDBm, args.Advertisement),
            AdStructures = ParseAdStructures(args.Advertisement)
        };

        if (_activeFilters != null && !DeviceFilter.MatchesAll(device, _activeFilters))
            return;

        ScanResult?.Invoke(this, new ScanResultEvent { Device = device });
    }

    public async Task ConnectAsync(string deviceId)
    {
        if (!ulong.TryParse(deviceId, System.Globalization.NumberStyles.HexNumber, null, out var addr))
            throw new BleException(BleErrorCode.ConnectionFailed, $"Invalid device ID: {deviceId}");

        var device = await BluetoothLEDevice.FromBluetoothAddressAsync(addr);
        if (device == null)
            throw new BleException(BleErrorCode.ConnectionFailed, $"Cannot connect to {deviceId}");

        _connectedDevices[deviceId] = device;
        device.ConnectionStatusChanged += (_, _) =>
        {
            ConnectionStateChanged?.Invoke(this, new ConnectionStateEvent
            {
                DeviceId = deviceId,
                Address = ConvertToMacAddress(addr),
                IsConnected = device.ConnectionStatus == BluetoothConnectionStatus.Connected
            });
        };

        ConnectionStateChanged?.Invoke(this, new ConnectionStateEvent
        {
            DeviceId = deviceId,
            Address = ConvertToMacAddress(addr),
            IsConnected = true
        });
    }

    public Task DisconnectAsync(string deviceId)
    {
        if (_connectedDevices.TryGetValue(deviceId, out var device))
        {
            device.Dispose();
            _connectedDevices.Remove(deviceId);
        }
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<BleServiceInfo>> DiscoverServicesAsync(string deviceId)
    {
        if (!_connectedDevices.TryGetValue(deviceId, out var device))
            throw new BleException(BleErrorCode.NotConnected, $"Device {deviceId} is not connected.");

        var result = await device.GetGattServicesAsync(BluetoothCacheMode.Uncached);
        if (result.Status != GattCommunicationStatus.Success)
            throw new BleException(BleErrorCode.ServiceNotFound, $"Failed to discover services on {deviceId}: {result.Status}");

        var services = new List<BleServiceInfo>();
        foreach (var gattService in result.Services)
        {
            var serviceInfo = new BleServiceInfo { Uuid = gattService.Uuid.ToString("D") };
            var charResult = await gattService.GetCharacteristicsAsync(BluetoothCacheMode.Uncached);
            if (charResult.Status == GattCommunicationStatus.Success)
            {
                foreach (var ch in charResult.Characteristics)
                {
                    var perms = GattPermissions.None;
                    var props = ch.CharacteristicProperties;
                    if (props.HasFlag(GattCharacteristicProperties.Read)) perms |= GattPermissions.Read;
                    if (props.HasFlag(GattCharacteristicProperties.Write)) perms |= GattPermissions.Write;
                    if (props.HasFlag(GattCharacteristicProperties.WriteWithoutResponse)) perms |= GattPermissions.WriteWithoutResponse;
                    if (props.HasFlag(GattCharacteristicProperties.Notify)) perms |= GattPermissions.Notify;
                    if (props.HasFlag(GattCharacteristicProperties.Indicate)) perms |= GattPermissions.Indicate;

                    serviceInfo.Characteristics.Add(new BleCharacteristicInfo
                    {
                        Uuid = ch.Uuid.ToString("D"),
                        ServiceUuid = serviceInfo.Uuid,
                        Permissions = perms
                    });
                }
            }
            services.Add(serviceInfo);
        }
        return services;
    }

    public async Task<byte[]> ReadCharacteristicAsync(string deviceId, string serviceUuid, string characteristicUuid)
    {
        var ch = await GetCharacteristicAsync(deviceId, serviceUuid, characteristicUuid);
        if (!ch.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Read))
            throw new BleException(BleErrorCode.CharacteristicNotReadable, $"Characteristic {characteristicUuid} is not readable.");

        var result = await ch.ReadValueAsync(BluetoothCacheMode.Uncached);
        if (result.Status != GattCommunicationStatus.Success)
            throw new BleException(BleErrorCode.GattReadFailed, $"Read failed: {result.Status}");

        return ToByteArray(result.Value);
    }

    public async Task WriteCharacteristicAsync(string deviceId, string serviceUuid, string characteristicUuid, byte[] data, bool withResponse = true)
    {
        var ch = await GetCharacteristicAsync(deviceId, serviceUuid, characteristicUuid);
        var expected = withResponse ? GattCharacteristicProperties.Write : GattCharacteristicProperties.WriteWithoutResponse;
        if (!ch.CharacteristicProperties.HasFlag(expected))
            throw new BleException(BleErrorCode.CharacteristicNotWritable, $"Characteristic {characteristicUuid} is not writable.");

        var writer = new DataWriter();
        writer.WriteBytes(data);
        var status = await ch.WriteValueAsync(writer.DetachBuffer(),
            withResponse ? GattWriteOption.WriteWithResponse : GattWriteOption.WriteWithoutResponse);
        if (status != GattCommunicationStatus.Success)
            throw new BleException(BleErrorCode.GattWriteFailed, $"Write failed: {status}");
    }

    public async Task SubscribeCharacteristicAsync(string deviceId, string serviceUuid, string characteristicUuid)
    {
        var ch = await GetCharacteristicAsync(deviceId, serviceUuid, characteristicUuid);
        if (!ch.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Notify)
            && !ch.CharacteristicProperties.HasFlag(GattCharacteristicProperties.Indicate))
            throw new BleException(BleErrorCode.CharacteristicNotNotifiable, $"Characteristic {characteristicUuid} does not support notifications.");

        var status = await ch.WriteClientCharacteristicConfigurationDescriptorAsync(
            GattClientCharacteristicConfigurationDescriptorValue.Notify);
        if (status != GattCommunicationStatus.Success)
            throw new BleException(BleErrorCode.SubscribeFailed, $"Subscribe failed: {status}");

        ch.ValueChanged += (_, e) =>
        {
            NotificationReceived?.Invoke(this, new NotificationEvent
            {
                DeviceId = deviceId,
                ServiceUuid = serviceUuid,
                CharacteristicUuid = characteristicUuid,
                Data = ToByteArray(e.CharacteristicValue)
            });
        };

        if (!_subscribedChars.ContainsKey(deviceId))
            _subscribedChars[deviceId] = new();
        _subscribedChars[deviceId][characteristicUuid] = ch;
    }

    public async Task UnsubscribeCharacteristicAsync(string deviceId, string serviceUuid, string characteristicUuid)
    {
        var ch = await GetCharacteristicAsync(deviceId, serviceUuid, characteristicUuid);
        await ch.WriteClientCharacteristicConfigurationDescriptorAsync(
            GattClientCharacteristicConfigurationDescriptorValue.None);
        if (_subscribedChars.TryGetValue(deviceId, out var chars))
            chars.Remove(characteristicUuid);
    }

    private async Task<GattCharacteristic> GetCharacteristicAsync(string deviceId, string serviceUuid, string characteristicUuid)
    {
        if (!_connectedDevices.TryGetValue(deviceId, out var device))
            throw new BleException(BleErrorCode.NotConnected, $"Device {deviceId} is not connected.");

        var svcResult = await device.GetGattServicesAsync(BluetoothCacheMode.Uncached);
        if (svcResult.Status != GattCommunicationStatus.Success)
            throw new BleException(BleErrorCode.ServiceNotFound, $"Failed to get services for {deviceId}: {svcResult.Status}");

        var serviceGuid = Guid.Parse(serviceUuid);
        var gattSvc = svcResult.Services.FirstOrDefault(s => s.Uuid == serviceGuid);
        if (gattSvc == null)
            throw new BleException(BleErrorCode.ServiceNotFound, $"Service {serviceUuid} not found.");

        var chResult = await gattSvc.GetCharacteristicsAsync(BluetoothCacheMode.Uncached);
        if (chResult.Status != GattCommunicationStatus.Success)
            throw new BleException(BleErrorCode.CharacteristicNotFound, $"Characteristic {characteristicUuid} not found.");

        foreach (var ch in chResult.Characteristics)
        {
            if (ch.Uuid == Guid.Parse(characteristicUuid))
                return ch;
        }
        throw new BleException(BleErrorCode.CharacteristicNotFound, $"Characteristic {characteristicUuid} not found.");
    }

    private static string ConvertToMacAddress(ulong address)
    {
        var bytes = BitConverter.GetBytes(address);
        return $"{bytes[5]:X2}:{bytes[4]:X2}:{bytes[3]:X2}:{bytes[2]:X2}:{bytes[1]:X2}:{bytes[0]:X2}";
    }

    private static byte[] ToByteArray(IBuffer buffer)
    {
        using var reader = DataReader.FromBuffer(buffer);
        var bytes = new byte[buffer.Length];
        reader.ReadBytes(bytes);
        return bytes;
    }

    private static byte[] ToByteArray(int rssi, BluetoothLEAdvertisement ad)
    {
        var list = new List<byte>();
        list.AddRange(BitConverter.GetBytes(rssi));
        foreach (var section in ad.DataSections)
        {
            list.Add(section.DataType);
            var data = ToByteArray(section.Data);
            list.Add((byte)data.Length);
            list.AddRange(data);
        }
        return list.ToArray();
    }

    private static Dictionary<ushort, byte[]> ParseAdStructures(BluetoothLEAdvertisement ad)
    {
        var dict = new Dictionary<ushort, byte[]>();
        foreach (var section in ad.DataSections)
        {
            dict[(ushort)section.DataType] = ToByteArray(section.Data);
        }
        return dict;
    }

    public void Dispose()
    {
        _watcher.Stop();
        foreach (var device in _connectedDevices.Values)
            device.Dispose();
        _connectedDevices.Clear();
    }
}
