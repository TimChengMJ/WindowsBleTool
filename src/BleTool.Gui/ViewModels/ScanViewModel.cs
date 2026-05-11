using System.Collections.ObjectModel;
using BleCore;
using BleCore.Models;
using BleTool.Shared.DataFormat;
using BleTool.Shared.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace BleTool.Gui.ViewModels;

public partial class ScanViewModel : ObservableObject
{
    private readonly BleScanner _scanner = new();
    private readonly SessionLogger _logger = new();

    [ObservableProperty] private bool _isScanning;
    [ObservableProperty] private ObservableCollection<BleDeviceInfo> _devices = new();
    [ObservableProperty] private ObservableCollection<BleDeviceInfo> _filteredDevices = new();
    [ObservableProperty] private BleDeviceInfo? _selectedDevice;
    [ObservableProperty] private AdvertisementData? _selectedAdData;
    [ObservableProperty] private ObservableCollection<ScanFilter> _activeFilters = new();
    [ObservableProperty] private FilterLogic _filterLogic = FilterLogic.And;
    [ObservableProperty] private ObservableCollection<GattServiceInfo> _services = new();
    [ObservableProperty] private GattServiceInfo? _selectedService;
    [ObservableProperty] private ObservableCollection<GattCharacteristicInfo> _characteristics = new();
    [ObservableProperty] private string _valueHex = string.Empty;
    [ObservableProperty] private string _valueDec = string.Empty;
    [ObservableProperty] private string _valueBin = string.Empty;
    [ObservableProperty] private string _valueUtf8 = string.Empty;
    [ObservableProperty] private string _valueBase64 = string.Empty;
    [ObservableProperty] private DataFormatType _selectedFormat = DataFormatType.Hex;
    [ObservableProperty] private ObservableCollection<string> _logEntries = new();
    [ObservableProperty] private BleDevice? _connectedDevice;

    public ScanViewModel()
    {
        _scanner.DeviceDiscovered += OnDeviceDiscovered;
        _scanner.DeviceUpdated += OnDeviceUpdated;
        _logger.EntryAdded += e =>
            DispatcherQueue.TryEnqueue(() => _logEntries.Add(e.ToString()));
    }

    [RelayCommand]
    private void StartScan()
    {
        Devices.Clear();
        FilteredDevices.Clear();
        _scanner.Filters = ActiveFilters.ToList();
        _scanner.FilterLogic = FilterLogic;
        _scanner.Start();
        IsScanning = true;
        _logger.Log(LogCategory.Scan, "开始扫描");
    }

    [RelayCommand]
    private void StopScan()
    {
        _scanner.Stop();
        IsScanning = false;
        _logger.Log(LogCategory.Scan, "停止扫描");
    }

    [RelayCommand]
    private async Task ConnectToDevice(BleDeviceInfo device)
    {
        ConnectedDevice?.Disconnect();
        ConnectedDevice = new BleDevice(device);
        await ConnectedDevice.ConnectAsync();
        await DiscoverServices();
        _logger.Log(LogCategory.Connect, $"已连接: {device.Name}");
    }

    [RelayCommand]
    private async Task DiscoverServices()
    {
        if (ConnectedDevice == null) return;
        var services = await ConnectedDevice.DiscoverServicesAsync();
        Services.Clear();
        foreach (var svc in services) Services.Add(svc);
        _logger.Log(LogCategory.Gatt, $"发现 {services.Count} 个服务");
    }

    [RelayCommand]
    private async Task ReadCharacteristic(GattCharacteristicInfo ch)
    {
        if (ConnectedDevice == null || SelectedService == null) return;
        var data = await ConnectedDevice.ReadCharacteristicAsync(SelectedService.Uuid, ch.Uuid);
        UpdateValueDisplay(data);
        _logger.Log(LogCategory.Read, $"{ch.Name}: {ValueHex}");
    }

    [RelayCommand]
    private async Task WriteCharacteristic(GattCharacteristicInfo ch)
    {
        if (ConnectedDevice == null || SelectedService == null) return;
        var data = DataFormatter.ParseInput(ValueHex, DataFormatType.Hex);
        await ConnectedDevice.WriteCharacteristicAsync(SelectedService.Uuid, ch.Uuid, data);
        _logger.Log(LogCategory.Write, $"{ch.Name}: {ValueHex}");
    }

    [RelayCommand]
    private async Task SubscribeToCharacteristic(GattCharacteristicInfo ch)
    {
        if (ConnectedDevice == null || SelectedService == null) return;
        await ConnectedDevice.SubscribeAsync(SelectedService.Uuid, ch.Uuid, data =>
        {
            DispatcherQueue.TryEnqueue(() => UpdateValueDisplay(data));
        });
        _logger.Log(LogCategory.Subscribe, $"已订阅 {ch.Name} 通知");
    }

    private void UpdateValueDisplay(byte[] data)
    {
        var formats = DataFormatter.FormatAll(data);
        ValueHex = formats[0]; ValueDec = formats[1]; ValueBin = formats[2];
        ValueUtf8 = formats[3]; ValueBase64 = formats[4];
    }

    partial void OnSelectedDeviceChanged(BleDeviceInfo? value)
    {
        SelectedAdData = value?.AdvertisementData;
    }

    partial void OnActiveFiltersChanged(System.Collections.IList? value)
    {
        _scanner.Filters = ActiveFilters.ToList();
        FilteredDevices = new ObservableCollection<BleDeviceInfo>(
            _scanner.GetFilteredDevices());
    }

    private void OnDeviceDiscovered(BleDeviceInfo device)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            Devices.Add(device);
            FilteredDevices = new ObservableCollection<BleDeviceInfo>(
                _scanner.GetFilteredDevices());
        });
    }

    private void OnDeviceUpdated(BleDeviceInfo device)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            var idx = Devices.IndexOf(
                Devices.FirstOrDefault(d => d.BluetoothAddress == device.BluetoothAddress));
            if (idx >= 0) Devices[idx] = device;
            FilteredDevices = new ObservableCollection<BleDeviceInfo>(
                _scanner.GetFilteredDevices());
        });
    }

    public Microsoft.UI.Dispatching.DispatcherQueue DispatcherQueue { get; set; } =
        Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
}
