using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BleCore;
using System.Collections.ObjectModel;

namespace BleTool.Gui.ViewModels;

public partial class GattViewModel : ObservableObject
{
    private WindowsBleAdapter? _adapter;
    private SessionLogger? _logger;

    public ObservableCollection<BleServiceInfo> Services { get; } = new();
    public ObservableCollection<BleCharacteristicInfo> Characteristics { get; } = new();

    [ObservableProperty]
    private BleServiceInfo? _selectedService;

    [ObservableProperty]
    private BleCharacteristicInfo? _selectedCharacteristic;

    [ObservableProperty]
    private string _valueHex = "";

    [ObservableProperty]
    private string _valueDec = "";

    [ObservableProperty]
    private string _valueBin = "";

    [ObservableProperty]
    private string _valueUtf8 = "";

    [ObservableProperty]
    private string _valueBase64 = "";

    [ObservableProperty]
    private string _connectedDeviceId = "";

    [ObservableProperty]
    private string _connectedDeviceName = "";

    [ObservableProperty]
    private bool _isDevMode = true;

    public void Initialize(WindowsBleAdapter adapter, SessionLogger logger)
    {
        _adapter = adapter;
        _logger = logger;
    }

    public async Task ConnectAndDiscover(string deviceId, string deviceName)
    {
        if (_adapter == null) return;
        ConnectedDeviceId = deviceId;
        ConnectedDeviceName = deviceName;
        _logger?.Log("CONNECT", $"连接到 {deviceName} ({deviceId})");

        await _adapter.ConnectAsync(deviceId);
        var services = await _adapter.DiscoverServicesAsync(deviceId);
        Services.Clear();
        foreach (var svc in services) Services.Add(svc);
        _logger?.Log("GATT", $"发现 {services.Count} 个服务");
    }

    partial void OnSelectedServiceChanged(BleServiceInfo? value)
    {
        if (value != null)
        {
            Characteristics.Clear();
            foreach (var ch in value.Characteristics) Characteristics.Add(ch);
        }
    }

    [RelayCommand]
    public async Task ReadCharacteristic()
    {
        if (_adapter == null || SelectedCharacteristic == null) return;
        _logger?.Log("READ", $"读取 {SelectedCharacteristic.Uuid}");
        var data = await _adapter.ReadCharacteristicAsync(
            ConnectedDeviceId, SelectedCharacteristic.ServiceUuid, SelectedCharacteristic.Uuid);
        UpdateValues(data);
    }

    [RelayCommand]
    public async Task WriteCharacteristic()
    {
        if (_adapter == null || SelectedCharacteristic == null) return;
        var bytes = HexToBytes(ValueHex);
        _logger?.Log("WRITE", $"写入 {SelectedCharacteristic.Uuid}");
        await _adapter.WriteCharacteristicAsync(
            ConnectedDeviceId, SelectedCharacteristic.ServiceUuid, SelectedCharacteristic.Uuid, bytes);
    }

    [RelayCommand]
    public async Task SubscribeCharacteristic()
    {
        if (_adapter == null || SelectedCharacteristic == null) return;
        _logger?.Log("SUBSCRIBE", $"订阅 {SelectedCharacteristic.Uuid}");
        _adapter.NotificationReceived += OnNotification;
        await _adapter.SubscribeCharacteristicAsync(
            ConnectedDeviceId, SelectedCharacteristic.ServiceUuid, SelectedCharacteristic.Uuid);
    }

    [RelayCommand]
    public async Task UnsubscribeCharacteristic()
    {
        if (_adapter == null || SelectedCharacteristic == null) return;
        _adapter.NotificationReceived -= OnNotification;
        await _adapter.UnsubscribeCharacteristicAsync(
            ConnectedDeviceId, SelectedCharacteristic.ServiceUuid, SelectedCharacteristic.Uuid);
    }

    private void OnNotification(object? s, NotificationEvent e)
    {
        if (e.CharacteristicUuid == SelectedCharacteristic?.Uuid)
        {
            Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread()?.TryEnqueue(
                () => UpdateValues(e.Data));
        }
    }

    private void UpdateValues(byte[] data)
    {
        ValueHex = DataFormatter.Format(data, DataFormat.Hex);
        ValueDec = DataFormatter.Format(data, DataFormat.Decimal);
        ValueBin = DataFormatter.Format(data, DataFormat.Binary);
        ValueUtf8 = DataFormatter.Format(data, DataFormat.Utf8);
        ValueBase64 = DataFormatter.Format(data, DataFormat.Base64);
    }

    private static byte[] HexToBytes(string hex)
    {
        hex = hex.Replace(" ", "").Replace("-", "");
        var bytes = new byte[hex.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
            bytes[i] = byte.Parse(hex.AsSpan(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
        return bytes;
    }

    public void SetDevMode(bool isDev) => IsDevMode = isDev;
}
