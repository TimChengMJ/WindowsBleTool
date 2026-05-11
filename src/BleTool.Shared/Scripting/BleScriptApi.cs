using BleCore;
using BleCore.Models;

namespace BleTool.Shared.Scripting;

public class BleScriptApi
{
    private readonly BleScanner _scanner;
    private readonly Dictionary<string, BleDevice> _devices = new();
    public Action<string>? ConsoleLogCallback { get; set; }

    public BleScriptApi(BleScanner scanner)
    {
        _scanner = scanner;
    }

    public object scanAsync(object filtersArg, object optsArg)
    {
        return ScanAsync(filtersArg, optsArg);
    }

    private async Task<object> ScanAsync(object filtersArg, object optsArg)
    {
        var filters = ParseScanFilters(filtersArg);
        var opts = ParseScanOptions(optsArg);
        var duration = opts.ContainsKey("duration") ? (int)opts["duration"] : 5000;

        _scanner.Filters = filters;
        _scanner.Start();

        await Task.Delay(duration);
        _scanner.Stop();

        var devices = _scanner.AllDevices
            .Select(d => new
            {
                name = d.Name,
                address = d.AddressString,
                rssi = d.Rssi,
                deviceId = d.DeviceId
            })
            .ToList<object>();

        return devices;
    }

    public object connectAsync(string address)
    {
        return ConnectAsync(address);
    }

    private async Task<object> ConnectAsync(string address)
    {
        var device = _scanner.AllDevices.FirstOrDefault(d =>
            d.AddressString.Equals(address, StringComparison.OrdinalIgnoreCase));
        if (device == null)
            throw new InvalidOperationException($"设备 {address} 未在扫描列表中找到");

        var bleDevice = new BleDevice(device);
        await bleDevice.ConnectAsync();
        _devices[address] = bleDevice;

        return new BleDeviceScriptWrapper(bleDevice);
    }

    public object getConnectedDevices()
    {
        return _devices.Keys.Select(addr => new { address = addr }).ToList();
    }

    private static List<ScanFilter> ParseScanFilters(object filtersArg)
    {
        var filters = new List<ScanFilter>();
        return filters;
    }

    private static Dictionary<string, object> ParseScanOptions(object optsArg)
    {
        if (optsArg is IDictionary<string, object> dict)
            return dict.ToDictionary(k => k.Key, v => v.Value);
        return new Dictionary<string, object>();
    }
}

public class BleDeviceScriptWrapper
{
    private readonly BleDevice _device;

    public BleDeviceScriptWrapper(BleDevice device) { _device = device; }

    public object getServiceAsync(string uuid)
    {
        return GetServiceAsync(uuid);
    }

    private async Task<object> GetServiceAsync(string uuid)
    {
        var services = await _device.DiscoverServicesAsync();
        var svc = services.FirstOrDefault(s =>
            s.Uuid.ToString("D").Contains(uuid, StringComparison.OrdinalIgnoreCase));
        if (svc == null) throw new InvalidOperationException($"Service {uuid} not found");
        return new BleServiceWrapper(_device, svc);
    }

    public void disconnect() => _device.Disconnect();
}

public class BleServiceWrapper
{
    private readonly BleDevice _device;
    private readonly GattServiceInfo _service;

    public BleServiceWrapper(BleDevice device, GattServiceInfo service)
    {
        _device = device;
        _service = service;
    }

    public object getCharacteristicAsync(string uuid)
    {
        return GetCharacteristicAsync(uuid);
    }

    private async Task<object> GetCharacteristicAsync(string uuid)
    {
        var ch = _service.Characteristics.FirstOrDefault(c =>
            c.Uuid.ToString("D").Contains(uuid, StringComparison.OrdinalIgnoreCase));
        if (ch == null) throw new InvalidOperationException($"Characteristic {uuid} not found");
        return new BleCharWrapper(_device, _service.Uuid, ch);
    }
}

public class BleCharWrapper
{
    private readonly BleDevice _device;
    private readonly Guid _serviceUuid;
    private readonly GattCharacteristicInfo _characteristic;

    public BleCharWrapper(BleDevice device, Guid serviceUuid, GattCharacteristicInfo characteristic)
    {
        _device = device;
        _serviceUuid = serviceUuid;
        _characteristic = characteristic;
    }

    public object readAsync()
    {
        return _device.ReadCharacteristicAsync(_serviceUuid, _characteristic.Uuid)!;
    }

    public object writeAsync(byte[] data)
    {
        return _device.WriteCharacteristicAsync(_serviceUuid, _characteristic.Uuid, data, withResponse: true)!;
    }

    public object writeWithoutResponseAsync(byte[] data)
    {
        return _device.WriteCharacteristicAsync(_serviceUuid, _characteristic.Uuid, data, withResponse: false)!;
    }

    public object subscribeAsync(dynamic callback)
    {
        return _device.SubscribeAsync(_serviceUuid, _characteristic.Uuid, data =>
        {
            try { callback(data); }
            catch { /* callback failures don't crash host */ }
        });
    }

    public void unsubscribe()
    {
        _device.UnsubscribeAsync(_characteristic.Uuid).GetAwaiter().GetResult();
    }
}
