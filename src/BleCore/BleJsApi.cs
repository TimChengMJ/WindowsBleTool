// src/BleCore/BleJsApi.cs
namespace BleCore;

public class BleJsApi
{
    private readonly IBleAdapter _adapter;
    private readonly Action<string, bool> _consoleLog;

    public BleJsApi(IBleAdapter adapter, Action<string, bool> consoleLog)
    {
        _adapter = adapter;
        _consoleLog = consoleLog;
    }

    public async Task<object> ScanAsync(IDictionary<string, object>? opts)
    {
        var filters = ParseFilters(opts);
        var duration = opts != null && opts.TryGetValue("duration", out var d) && d is int dur ? dur : 5000;
        var devices = new List<JsDeviceInfo>();

        void handler(object? s, ScanResultEvent e)
        {
            lock (devices)
            {
                if (!devices.Any(d => d.Address == e.Device.Address))
                    devices.Add(new JsDeviceInfo(_adapter, e.Device.Address, e.Device.Name, e.Device.Rssi));
            }
        }

        _adapter.ScanResult += handler;
        await _adapter.StartScanAsync(filters);

        try { await Task.Delay(duration); }
        finally
        {
            _adapter.StopScan();
            _adapter.ScanResult -= handler;
        }
        return devices;
    }

    public async Task<object> ConnectAsync(string address)
    {
        await _adapter.ConnectAsync(address);
        return new JsDeviceInfo(_adapter, address, address, 0);
    }

    public object GetConnectedDevice(string address)
        => new JsDeviceInfo(_adapter, address, address, 0);

    private static List<DeviceFilterRule>? ParseFilters(IDictionary<string, object>? opts)
    {
        if (opts == null || !opts.TryGetValue("filters", out var filtersRaw) || filtersRaw is not IList<object> filterList)
            return null;

        var rules = new List<DeviceFilterRule>();
        foreach (var filter in filterList)
        {
            if (filter is not IDictionary<string, object> f) continue;

            if (f.TryGetValue("name", out var nameObj))
            {
                rules.Add(new DeviceFilterRule
                {
                    Type = FilterType.DeviceName,
                    Operator = FilterOperator.Contains,
                    Value = nameObj.ToString() ?? ""
                });
            }
            else if (f.TryGetValue("rssi", out var rssiObj) && int.TryParse(rssiObj?.ToString(), out var rssiVal))
            {
                rules.Add(new DeviceFilterRule
                {
                    Type = FilterType.Rssi,
                    Operator = FilterOperator.GreaterThanOrEqual,
                    Value = rssiVal.ToString()
                });
            }
        }
        return rules;
    }

    public void ConsoleLog(string message) => _consoleLog(message, false);
    public void ConsoleError(string message) => _consoleLog(message, true);
}

public class JsDeviceInfo
{
    private readonly IBleAdapter _adapter;
    private readonly string _deviceId;

    public string Address { get; }
    public string Name { get; }
    public int Rssi { get; }

    public JsDeviceInfo(IBleAdapter adapter, string address, string name, int rssi)
    {
        _adapter = adapter;
        _deviceId = address;
        Address = address;
        Name = name;
        Rssi = rssi;
    }

    public async Task<object> GetServiceAsync(string uuid)
    {
        var services = await _adapter.DiscoverServicesAsync(_deviceId);
        var svc = services.FirstOrDefault(s => s.Uuid.Equals(uuid, StringComparison.OrdinalIgnoreCase));
        if (svc == null) throw new Exception($"Service {uuid} not found");
        return new JsServiceInfo(_adapter, _deviceId, svc.Uuid);
    }

    public async Task DisconnectAsync()
    {
        await _adapter.DisconnectAsync(_deviceId);
    }
}

public class JsServiceInfo
{
    private readonly IBleAdapter _adapter;
    private readonly string _deviceId;

    public string Uuid { get; }

    public JsServiceInfo(IBleAdapter adapter, string deviceId, string uuid)
    {
        _adapter = adapter;
        _deviceId = deviceId;
        Uuid = uuid;
    }

    public async Task<object> GetCharacteristicAsync(string uuid)
    {
        var services = await _adapter.DiscoverServicesAsync(_deviceId);
        var svc = services.FirstOrDefault(s => s.Uuid.Equals(Uuid, StringComparison.OrdinalIgnoreCase));
        if (svc == null) throw new Exception($"Service {Uuid} not found");

        var ch = svc.Characteristics.FirstOrDefault(c => c.Uuid.Equals(uuid, StringComparison.OrdinalIgnoreCase));
        if (ch == null) throw new Exception($"Characteristic {uuid} not found");

        return new JsCharacteristicInfo(_adapter, _deviceId, Uuid, ch.Uuid);
    }
}

public class JsCharacteristicInfo
{
    private readonly IBleAdapter _adapter;
    private readonly string _deviceId;

    public string Uuid { get; }
    public string ServiceUuid { get; }

    public JsCharacteristicInfo(IBleAdapter adapter, string deviceId, string serviceUuid, string uuid)
    {
        _adapter = adapter;
        _deviceId = deviceId;
        ServiceUuid = serviceUuid;
        Uuid = uuid;
    }

    public async Task<byte[]> ReadAsync()
        => await _adapter.ReadCharacteristicAsync(_deviceId, ServiceUuid, Uuid);

    public async Task WriteAsync(byte[] data)
        => await _adapter.WriteCharacteristicAsync(_deviceId, ServiceUuid, Uuid, data, withResponse: true);

    public async Task WriteWithoutResponseAsync(byte[] data)
        => await _adapter.WriteCharacteristicAsync(_deviceId, ServiceUuid, Uuid, data, withResponse: false);

    public async Task SubscribeAsync(dynamic callback)
    {
        _adapter.NotificationReceived += OnNotification;
        await _adapter.SubscribeCharacteristicAsync(_deviceId, ServiceUuid, Uuid);

        void OnNotification(object? s, NotificationEvent e)
        {
            if (e.CharacteristicUuid == Uuid && e.ServiceUuid == ServiceUuid)
            {
                try { callback(e.Data); } catch { /* swallow callback errors */ }
            }
        }
    }

    public async Task UnsubscribeAsync()
    {
        await _adapter.UnsubscribeCharacteristicAsync(_deviceId, ServiceUuid, Uuid);
    }
}
