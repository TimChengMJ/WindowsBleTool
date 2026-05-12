using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BleCore;
using System.Collections.ObjectModel;

namespace BleTool.Gui.ViewModels;

public partial class ScanViewModel : ObservableObject
{
    private WindowsBleAdapter? _adapter;
    private SessionLogger? _logger;
    private bool _isScanning;

    public ObservableCollection<BleDeviceInfo> Devices { get; } = new();
    public ObservableCollection<DeviceFilterRule> ActiveFilters { get; } = new();
    public ObservableCollection<ParsedAdField> AdData { get; } = new();

    [ObservableProperty]
    private BleDeviceInfo? _selectedDevice;

    [ObservableProperty]
    private string _matchCount = "0 台设备";

    [ObservableProperty]
    private bool _isDevMode = true;

    public void Initialize(WindowsBleAdapter adapter, SessionLogger logger)
    {
        _adapter = adapter;
        _logger = logger;
        _adapter.ScanResult += OnScanResult;
    }

    private void OnScanResult(object? s, ScanResultEvent e)
    {
        Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread()?.TryEnqueue(() =>
        {
            var existing = Devices.FirstOrDefault(d => d.Address == e.Device.Address);
            if (existing != null)
            {
                var idx = Devices.IndexOf(existing);
                Devices[idx] = e.Device;
            }
            else
            {
                Devices.Add(e.Device);
            }
            MatchCount = $"{Devices.Count} 台设备";
        });
    }

    [RelayCommand]
    public async Task StartScan()
    {
        if (_adapter == null || _isScanning) return;
        _isScanning = true;
        Devices.Clear();
        _logger?.Log("SCAN", "开始扫描");
        await _adapter.StartScanAsync(ActiveFilters.ToList());
    }

    [RelayCommand]
    public void StopScan()
    {
        _adapter?.StopScan();
        _isScanning = false;
        _logger?.Log("SCAN", "停止扫描");
    }

    [RelayCommand]
    public void AddFilter(DeviceFilterRule rule) => ActiveFilters.Add(rule);

    [RelayCommand]
    public void RemoveFilter(DeviceFilterRule rule) => ActiveFilters.Remove(rule);

    partial void OnSelectedDeviceChanged(BleDeviceInfo? value)
    {
        if (value != null)
        {
            var fields = AdDataParser.Parse(value.AdStructures);
            AdData.Clear();
            foreach (var f in fields) AdData.Add(f);
        }
    }

    public void SetDevMode(bool isDev) => IsDevMode = isDev;
}
