using System.Collections.ObjectModel;
using BleCore;
using BleCore.Models;
using BleTool.Shared.DataFormat;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace BleTool.Gui.ViewModels;

public partial class GattBrowserViewModel : ObservableObject
{
    [ObservableProperty] private BleDevice? _device;
    [ObservableProperty] private ObservableCollection<GattServiceInfo> _services = new();
    [ObservableProperty] private GattCharacteristicInfo? _selectedCharacteristic;
    [ObservableProperty] private string _valueHex = string.Empty;
    [ObservableProperty] private DataFormatType _selectedFormat = DataFormatType.Hex;

    public async Task SetDevice(BleDevice device)
    {
        _device = device;
        var services = await device.DiscoverServicesAsync();
        Services.Clear();
        foreach (var svc in services) Services.Add(svc);
    }

    [RelayCommand]
    private async Task RefreshServices()
    {
        if (Device == null) return;
        await SetDevice(Device);
    }

    [RelayCommand]
    private async Task ReadValue(GattCharacteristicInfo ch)
    {
        if (Device == null) return;
        var data = await Device.ReadCharacteristicAsync(
            FindParentService(ch).Uuid, ch.Uuid);
        ValueHex = DataFormatter.Format(data, SelectedFormat);
    }

    private GattServiceInfo FindParentService(GattCharacteristicInfo ch)
    {
        return Services.FirstOrDefault(s => s.Characteristics.Contains(ch))
            ?? throw new InvalidOperationException("Parent service not found");
    }
}
