using BleCore.Models;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BleCore;

public class BleDevice : IDisposable
{
    private BluetoothLEDevice? _ledevice;
    private readonly Dictionary<Guid, GattCharacteristic> _notificationRegistrations = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    public BleDeviceInfo Info { get; private set; }
    public bool IsConnected => _ledevice?.ConnectionStatus == BluetoothConnectionStatus.Connected;

    public event Action<string>? StatusChanged;
    public event Action<byte[], Guid>? NotificationReceived;

    public BleDevice(BleDeviceInfo info)
    {
        Info = info;
    }

    public async Task ConnectAsync()
    {
        await _lock.WaitAsync();
        try
        {
            _ledevice = await BluetoothLEDevice.FromBluetoothAddressAsync(Info.BluetoothAddress);
            if (_ledevice == null)
                throw new InvalidOperationException($"无法连接到设备 {Info.AddressString}");

            StatusChanged?.Invoke($"已连接到 {Info.Name}");
        }
        finally { _lock.Release(); }
    }

    public async Task<IReadOnlyList<GattServiceInfo>> DiscoverServicesAsync()
    {
        if (_ledevice == null) throw new InvalidOperationException("设备未连接");

        var result = await _ledevice.GetGattServicesAsync(BluetoothCacheMode.Uncached);
        if (result.Status != GattCommunicationStatus.Success)
            throw new InvalidOperationException($"GATT 服务发现失败: {result.Status}");

        var services = new List<GattServiceInfo>();
        foreach (var svc in result.Services)
        {
            var chars = await DiscoverCharacteristicsAsync(svc);
            services.Add(new GattServiceInfo
            {
                Uuid = svc.Uuid,
                Characteristics = chars
            });
        }
        return services;
    }

    private async Task<IReadOnlyList<GattCharacteristicInfo>> DiscoverCharacteristicsAsync(GattDeviceService service)
    {
        var result = await service.GetCharacteristicsAsync(BluetoothCacheMode.Uncached);
        if (result.Status != GattCommunicationStatus.Success)
            return Array.Empty<GattCharacteristicInfo>();

        var chars = new List<GattCharacteristicInfo>();
        foreach (var ch in result.Characteristics)
        {
            chars.Add(new GattCharacteristicInfo
            {
                Uuid = ch.Uuid,
                Properties = ch.CharacteristicProperties.ToString()
            });
        }
        return chars;
    }

    public async Task<byte[]> ReadCharacteristicAsync(Guid serviceUuid, Guid characteristicUuid)
    {
        var ch = await GetCharacteristicAsync(serviceUuid, characteristicUuid);
        var result = await ch.ReadValueAsync(BluetoothCacheMode.Uncached);
        if (result.Status != GattCommunicationStatus.Success)
            throw new InvalidOperationException($"读取失败: {result.Status}");

        using var reader = Windows.Storage.Streams.DataReader.FromBuffer(result.Value!);
        var data = new byte[reader.UnconsumedBufferLength];
        reader.ReadBytes(data);
        StatusChanged?.Invoke($"READ {characteristicUuid}: {BitConverter.ToString(data)}");
        return data;
    }

    public async Task WriteCharacteristicAsync(Guid serviceUuid, Guid characteristicUuid, byte[] data,
        bool withResponse = true)
    {
        var ch = await GetCharacteristicAsync(serviceUuid, characteristicUuid);
        var writer = new Windows.Storage.Streams.DataWriter();
        writer.WriteBytes(data);
        var result = await ch.WriteValueAsync(writer.DetachBuffer(),
            withResponse ? GattWriteOption.WriteWithResponse : GattWriteOption.WriteWithoutResponse);
        if (result != GattCommunicationStatus.Success)
            throw new InvalidOperationException($"写入失败: {result}");
        StatusChanged?.Invoke($"WRITE {characteristicUuid}: {BitConverter.ToString(data)}");
    }

    public async Task SubscribeAsync(Guid serviceUuid, Guid characteristicUuid, Action<byte[]> callback)
    {
        var ch = await GetCharacteristicAsync(serviceUuid, characteristicUuid);
        var status = await ch.WriteClientCharacteristicConfigurationDescriptorAsync(
            GattClientCharacteristicConfigurationDescriptorValue.Notify);
        if (status != GattCommunicationStatus.Success)
            throw new InvalidOperationException($"订阅失败: {status}");

        ch.ValueChanged += (s, e) =>
        {
            using var reader = Windows.Storage.Streams.DataReader.FromBuffer(e.CharacteristicValue!);
            var data = new byte[reader.UnconsumedBufferLength];
            reader.ReadBytes(data);
            callback(data);
            NotificationReceived?.Invoke(data, characteristicUuid);
        };
        _notificationRegistrations[characteristicUuid] = ch;
        StatusChanged?.Invoke($"已订阅 {characteristicUuid} 通知");
    }

    public async Task UnsubscribeAsync(Guid characteristicUuid)
    {
        if (_notificationRegistrations.TryGetValue(characteristicUuid, out var ch))
        {
            await ch.WriteClientCharacteristicConfigurationDescriptorAsync(
                GattClientCharacteristicConfigurationDescriptorValue.None);
            _notificationRegistrations.Remove(characteristicUuid);
            StatusChanged?.Invoke($"已取消订阅 {characteristicUuid}");
        }
    }

    public void Disconnect()
    {
        _ledevice?.Dispose();
        _ledevice = null;
        _notificationRegistrations.Clear();
        StatusChanged?.Invoke($"已断开 {Info.Name}");
    }

    private async Task<GattCharacteristic> GetCharacteristicAsync(Guid serviceUuid, Guid characteristicUuid)
    {
        if (_ledevice == null) throw new InvalidOperationException("设备未连接");
        var svcResult = await _ledevice.GetGattServicesForUuidAsync(serviceUuid, BluetoothCacheMode.Uncached);
        if (svcResult.Status != GattCommunicationStatus.Success || svcResult.Services.Count == 0)
            throw new InvalidOperationException($"服务 {serviceUuid} 未找到");
        var chResult = await svcResult.Services[0]
            .GetCharacteristicsForUuidAsync(characteristicUuid, BluetoothCacheMode.Uncached);
        if (chResult.Status != GattCommunicationStatus.Success || chResult.Characteristics.Count == 0)
            throw new InvalidOperationException($"特征值 {characteristicUuid} 未找到");
        return chResult.Characteristics[0];
    }

    public void Dispose()
    {
        Disconnect();
        _lock.Dispose();
    }
}
