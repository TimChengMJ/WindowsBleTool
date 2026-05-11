using BleCore.Models;
using Windows.Devices.Bluetooth.Advertisement;

namespace BleCore;

public class BleScanner
{
    private readonly BluetoothLEAdvertisementWatcher _watcher;
    private readonly List<BleDeviceInfo> _allDevices = new();
    private readonly Dictionary<ulong, BleDeviceInfo> _deviceMap = new();
    private readonly object _lock = new();

    public event Action<BleDeviceInfo>? DeviceDiscovered;
    public event Action<BleDeviceInfo>? DeviceUpdated;
    public event Action<string>? ScanStatusChanged;

    public bool IsScanning { get; private set; }
    public IReadOnlyList<BleDeviceInfo> AllDevices { get { lock(_lock) return _allDevices.ToList(); } }
    public IReadOnlyList<ScanFilter> Filters { get; set; } = Array.Empty<ScanFilter>();
    public FilterLogic FilterLogic { get; set; } = FilterLogic.And;

    public BleScanner()
    {
        _watcher = new BluetoothLEAdvertisementWatcher
        {
            ScanningMode = BluetoothLEScanningMode.Active
        };
        _watcher.Received += OnAdvertisementReceived;
        _watcher.Stopped += (s, e) =>
        {
            IsScanning = false;
            ScanStatusChanged?.Invoke("扫描已停止");
        };
    }

    public void Start()
    {
        lock (_lock) { _allDevices.Clear(); _deviceMap.Clear(); }
        _watcher.Start();
        IsScanning = true;
        ScanStatusChanged?.Invoke("扫描中...");
    }

    public void Stop()
    {
        _watcher.Stop();
        IsScanning = false;
        ScanStatusChanged?.Invoke("扫描已停止");
    }

    public IReadOnlyList<BleDeviceInfo> GetFilteredDevices()
    {
        lock (_lock)
        {
            return _allDevices
                .Where(d => ScanFilterEvaluator.Matches(d, Filters, FilterLogic))
                .OrderByDescending(d => d.Rssi)
                .ToList();
        }
    }

    private void OnAdvertisementReceived(BluetoothLEAdvertisementWatcher sender,
        BluetoothLEAdvertisementReceivedEventArgs args)
    {
        var address = args.BluetoothAddress;
        var adData = ParseAdvertisement(args.Advertisement);
        var deviceInfo = new BleDeviceInfo
        {
            DeviceId = args.BluetoothAddress.ToString(),
            Name = args.Advertisement.LocalName ?? "Unknown",
            BluetoothAddress = args.BluetoothAddress,
            Rssi = args.RawSignalStrengthInDBm,
            AdvertisementData = adData,
            LastSeen = args.Timestamp
        };

        lock (_lock)
        {
            if (_deviceMap.TryGetValue(address, out var existing))
            {
                _allDevices.Remove(existing);
            }
            _deviceMap[address] = deviceInfo;
            _allDevices.Add(deviceInfo);

            if (existing == null)
                DeviceDiscovered?.Invoke(deviceInfo);
            else
                DeviceUpdated?.Invoke(deviceInfo);
        }
    }

    private static AdvertisementData ParseAdvertisement(BluetoothLEAdvertisement ad)
    {
        var records = new List<AdvertisementRecord>();

        foreach (var section in ad.DataSections)
        {
            var rr = new byte[section.Data.Length];
            using var reader = Windows.Storage.Streams.DataReader.FromBuffer(section.Data);
            reader.ReadBytes(rr);
            records.Add(new AdvertisementRecord { AdType = (byte)section.DataType, Data = rr });
        }

        return new AdvertisementData { Records = records, Timestamp = DateTimeOffset.Now };
    }
}
