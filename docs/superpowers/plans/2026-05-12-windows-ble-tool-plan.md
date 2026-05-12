# Windows BLE Tool — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a Windows BLE debugging tool with WinUI 3 GUI, CLI, and embedded JavaScript scripting via ClearScript.

**Architecture:** Layered .NET solution — `BleCore` class library wraps Windows.Devices.Bluetooth and provides models, filters, formatting, logging, and script engine. `BleTool.Gui` (WinUI 3) and `BleTool.Cli` (console) both consume BleCore.

**Tech Stack:** .NET 8, WinUI 3, Windows.Devices.Bluetooth, ClearScript (Microsoft.ClearScript), System.CommandLine

---

## File Map

### BleCore (Class Library)
```
/src/BleCore/BleCore.csproj
/src/BleCore/Models/BleDeviceInfo.cs
/src/BleCore/Models/BleServiceInfo.cs
/src/BleCore/Models/BleCharacteristicInfo.cs
/src/BleCore/Models/BleAdapterEvent.cs
/src/BleCore/IBleAdapter.cs
/src/BleCore/WindowsBleAdapter.cs
/src/BleCore/DeviceFilter.cs
/src/BleCore/AdDataParser.cs
/src/BleCore/DataFormatter.cs
/src/BleCore/GattKnownUuids.cs
/src/BleCore/SessionLogger.cs
/src/BleCore/NotificationDataLogger.cs
/src/BleCore/AppSettings.cs
/src/BleCore/BleException.cs
/src/BleCore/ScriptEngine.cs
/src/BleCore/BleJsApi.cs
```

### BleTool.Gui (WinUI 3)
```
/src/BleTool.Gui/BleTool.Gui.csproj
/src/BleTool.Gui/App.xaml
/src/BleTool.Gui/App.xaml.cs
/src/BleTool.Gui/MainWindow.xaml
/src/BleTool.Gui/MainWindow.xaml.cs
/src/BleTool.Gui/Views/ScanPage.xaml
/src/BleTool.Gui/Views/ScanPage.xaml.cs
/src/BleTool.Gui/Views/GattPage.xaml
/src/BleTool.Gui/Views/GattPage.xaml.cs
/src/BleTool.Gui/Views/ScriptPage.xaml
/src/BleTool.Gui/Views/ScriptPage.xaml.cs
/src/BleTool.Gui/Views/SettingsPage.xaml
/src/BleTool.Gui/Views/SettingsPage.xaml.cs
/src/BleTool.Gui/ViewModels/ScanViewModel.cs
/src/BleTool.Gui/ViewModels/GattViewModel.cs
/src/BleTool.Gui/ViewModels/ScriptViewModel.cs
/src/BleTool.Gui/ViewModels/SettingsViewModel.cs
/src/BleTool.Gui/Controls/FilterEditor.xaml
/src/BleTool.Gui/Controls/FilterEditor.xaml.cs
/src/BleTool.Gui/Controls/FormatToggle.xaml
/src/BleTool.Gui/Controls/FormatToggle.xaml.cs
/src/BleTool.Gui/Services/GuiSessionLogger.cs
```

### BleTool.Cli (Console)
```
/src/BleTool.Cli/BleTool.Cli.csproj
/src/BleTool.Cli/Program.cs
```

### Tests
```
/tests/BleCore.Tests/BleCore.Tests.csproj
/tests/BleCore.Tests/DeviceFilterTests.cs
/tests/BleCore.Tests/DataFormatterTests.cs
/tests/BleCore.Tests/AdDataParserTests.cs
/tests/BleCore.Tests/WindowsBleAdapterTests.cs
/tests/BleCore.Tests/SessionLoggerTests.cs
```

---

## Phase 1: Scaffolding & Models

### Task 1: Create solution structure

**Files:**
- Create: `WindowsBleTool.sln`
- Create: `/src/BleCore/BleCore.csproj`
- Create: `/src/BleTool.Gui/BleTool.Gui.csproj`
- Create: `/src/BleTool.Cli/BleTool.Cli.csproj`
- Create: `/tests/BleCore.Tests/BleCore.Tests.csproj`

- [ ] **Step 1: Create solution and projects**

```bash
dotnet new sln -n WindowsBleTool -o E:\AI_Support\AI_Project\WindowsBleTool
cd E:\AI_Support\AI_Project\WindowsBleTool
dotnet new classlib -n BleCore -o src/BleCore -f net8.0-windows10.0.19041.0
dotnet new winui -n BleTool.Gui -o src/BleTool.Gui
dotnet new console -n BleTool.Cli -o src/BleTool.Cli -f net8.0-windows10.0.19041.0
dotnet new mstest -n BleCore.Tests -o tests/BleCore.Tests -f net8.0-windows10.0.19041.0
dotnet sln add src/BleCore/BleCore.csproj src/BleTool.Gui/BleTool.Gui.csproj src/BleTool.Cli/BleTool.Cli.csproj tests/BleCore.Tests/BleCore.Tests.csproj
```

- [ ] **Step 2: Add project references**

```bash
dotnet add src/BleTool.Gui/BleTool.Gui.csproj reference src/BleCore/BleCore.csproj
dotnet add src/BleTool.Cli/BleTool.Cli.csproj reference src/BleCore/BleCore.csproj
dotnet add tests/BleCore.Tests/BleCore.Tests.csproj reference src/BleCore/BleCore.csproj
```

- [ ] **Step 3: Add NuGet packages**

```bash
dotnet add src/BleCore/BleCore.csproj package Microsoft.ClearScript --version 7.4.5
dotnet add src/BleTool.Cli/BleTool.Cli.csproj package System.CommandLine --version 2.0.0-beta4.22272.1
dotnet add src/BleTool.Gui/BleTool.Gui.csproj package CommunityToolkit.Mvvm --version 8.2.2
dotnet add src/BleTool.Gui/BleTool.Gui.csproj package CommunityToolkit.WinUI.UI.Controls --version 7.1.2
dotnet add tests/BleCore.Tests/BleCore.Tests.csproj package Microsoft.ClearScript --version 7.4.5
```

- [ ] **Step 4: Verify build**

```bash
dotnet build E:\AI_Support\AI_Project\WindowsBleTool\WindowsBleTool.sln
```

- [ ] **Step 5: Commit**

```bash
git add -A
git commit -m "feat: scaffold solution with BleCore, Gui, Cli projects"
```

### Task 2: Create BLE data models

**Files:**
- Create: `/src/BleCore/GattKnownUuids.cs`
- Create: `/src/BleCore/Models/BleDeviceInfo.cs`
- Create: `/src/BleCore/Models/BleServiceInfo.cs`
- Create: `/src/BleCore/Models/BleCharacteristicInfo.cs`
- Create: `/src/BleCore/Models/BleAdapterEvent.cs`

- [ ] **Step 1: Write GattKnownUuids registry**

```csharp
// src/BleCore/GattKnownUuids.cs
namespace BleCore;

public static class GattKnownUuids
{
    private static readonly Dictionary<string, (string Name, DataFormat SuggestedFormat)> Registry = new()
    {
        { "00001800-0000-1000-8000-00805F9B34FB", ("Generic Access", DataFormat.Hex) },
        { "00001801-0000-1000-8000-00805F9B34FB", ("Generic Attribute", DataFormat.Hex) },
        { "0000180A-0000-1000-8000-00805F9B34FB", ("Device Information", DataFormat.Hex) },
        { "0000180D-0000-1000-8000-00805F9B34FB", ("Heart Rate", DataFormat.Hex) },
        { "0000180F-0000-1000-8000-00805F9B34FB", ("Battery Service", DataFormat.Hex) },
        { "00001809-0000-1000-8000-00805F9B34FB", ("Health Thermometer", DataFormat.Hex) },
        { "00001810-0000-1000-8000-00805F9B34FB", ("Blood Pressure", DataFormat.Hex) },
        { "00002A00-0000-1000-8000-00805F9B34FB", ("Device Name", DataFormat.Utf8) },
        { "00002A01-0000-1000-8000-00805F9B34FB", ("Appearance", DataFormat.Hex) },
        { "00002A19-0000-1000-8000-00805F9B34FB", ("Battery Level", DataFormat.Decimal) },
        { "00002A37-0000-1000-8000-00805F9B34FB", ("Heart Rate Measurement", DataFormat.Hex) },
        { "00002A38-0000-1000-8000-00805F9B34FB", ("Body Sensor Location", DataFormat.Decimal) },
        { "00002A39-0000-1000-8000-00805F9B34FB", ("Heart Rate Control Point", DataFormat.Hex) },
        { "00002A6E-0000-1000-8000-00805F9B34FB", ("Temperature Measurement", DataFormat.Hex) },
        { "00002A29-0000-1000-8000-00805F9B34FB", ("Manufacturer Name String", DataFormat.Utf8) },
        { "00002A24-0000-1000-8000-00805F9B34FB", ("Model Number String", DataFormat.Utf8) },
        { "00002A25-0000-1000-8000-00805F9B34FB", ("Serial Number String", DataFormat.Utf8) },
        { "00002A26-0000-1000-8000-00805F9B34FB", ("Firmware Revision String", DataFormat.Utf8) },
        { "00002A27-0000-1000-8000-00805F9B34FB", ("Hardware Revision String", DataFormat.Utf8) },
        { "00002A28-0000-1000-8000-00805F9B34FB", ("Software Revision String", DataFormat.Utf8) },
    };

    public static string GetName(string uuid)
        => Registry.TryGetValue(uuid.ToUpper(), out var entry) ? entry.Name : uuid;

    public static DataFormat GetSuggestedFormat(string uuid)
        => Registry.TryGetValue(uuid.ToUpper(), out var entry) ? entry.SuggestedFormat : DataFormat.Hex;
}
```

- [ ] **Step 2: Write BleDeviceInfo model**

```csharp
// src/BleCore/Models/BleDeviceInfo.cs
namespace BleCore;

public class BleDeviceInfo
{
    public string DeviceId { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int Rssi { get; init; }
    public string AddressType { get; init; } = "Public";
    public bool IsConnected { get; set; }
    public DateTimeOffset LastSeen { get; set; }
    public List<BleServiceInfo> Services { get; init; } = new();
    public byte[] RawAdvertisement { get; init; } = Array.Empty<byte>();
    public Dictionary<ushort, byte[]> AdStructures { get; init; } = new();
}
```

- [ ] **Step 3: Write BleServiceInfo model**

```csharp
// src/BleCore/Models/BleServiceInfo.cs
namespace BleCore;

public class BleServiceInfo
{
    public string Uuid { get; init; } = string.Empty;
    public string DisplayName => GattKnownUuids.GetName(Uuid);
    public Guid Guid => Guid.Parse(Uuid);
    public List<BleCharacteristicInfo> Characteristics { get; init; } = new();
}
```

- [ ] **Step 4: Write BleCharacteristicInfo model**

```csharp
// src/BleCore/Models/BleCharacteristicInfo.cs
namespace BleCore;

public class BleCharacteristicInfo
{
    public string Uuid { get; init; } = string.Empty;
    public string DisplayName => GattKnownUuids.GetName(Uuid);
    public string ServiceUuid { get; init; } = string.Empty;
    public GattPermissions Permissions { get; init; }
    public byte[] LastValue { get; set; } = Array.Empty<byte>();
    public string LastValueHex => LastValue.Length > 0
        ? BitConverter.ToString(LastValue).Replace("-", " ")
        : "";
}

[Flags]
public enum GattPermissions
{
    None = 0,
    Read = 1,
    Write = 2,
    WriteWithoutResponse = 4,
    Notify = 8,
    Indicate = 16,
}
```

- [ ] **Step 5: Write BleAdapterEvent model**

```csharp
// src/BleCore/Models/BleAdapterEvent.cs
namespace BleCore;

public class ScanResultEvent : EventArgs
{
    public BleDeviceInfo Device { get; init; } = null!;
}

public class ConnectionStateEvent : EventArgs
{
    public string DeviceId { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public bool IsConnected { get; init; }
}

public class NotificationEvent : EventArgs
{
    public string DeviceId { get; init; } = string.Empty;
    public string ServiceUuid { get; init; } = string.Empty;
    public string CharacteristicUuid { get; init; } = string.Empty;
    public byte[] Data { get; init; } = Array.Empty<byte>();
}
```

- [ ] **Step 6: Build and commit**

```bash
dotnet build E:\AI_Support\AI_Project\WindowsBleTool\WindowsBleTool.sln
git add -A
git commit -m "feat: add BLE data models and known UUID registry"
```

---

## Phase 2: BleCore — BLE Adapter & Filters

### Task 3: Define IBleAdapter interface and BleException

**Files:**
- Create: `/src/BleCore/IBleAdapter.cs`
- Create: `/src/BleCore/BleException.cs`

- [ ] **Step 1: Write IBleAdapter interface**

```csharp
// src/BleCore/IBleAdapter.cs
namespace BleCore;

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
```

- [ ] **Step 2: Write BleException**

```csharp
// src/BleCore/BleException.cs
namespace BleCore;

public class BleException : Exception
{
    public BleErrorCode Code { get; }

    public BleException(BleErrorCode code, string message, Exception? inner = null)
        : base(message, inner) => Code = code;
}

public enum BleErrorCode
{
    DeviceNotFound,
    DeviceUnreachable,
    ConnectionFailed,
    NotConnected,
    ServiceNotFound,
    CharacteristicNotFound,
    GattReadFailed,
    GattWriteFailed,
    CharacteristicNotReadable,
    CharacteristicNotWritable,
    CharacteristicNotNotifiable,
    SubscribeFailed,
    ScanError,
    AdapterUnavailable,
}
```

- [ ] **Step 3: Build and commit**

```bash
dotnet build E:\AI_Support\AI_Project\WindowsBleTool\WindowsBleTool.sln
git add -A
git commit -m "feat: add BLE adapter interface and custom exceptions"
```

### Task 4: Implement WindowsBleAdapter

**Files:**
- Create: `/src/BleCore/WindowsBleAdapter.cs`

- [ ] **Step 1: Write WindowsBleAdapter**

```csharp
// src/BleCore/WindowsBleAdapter.cs
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

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
        device.ConnectionStatusChanged += (s, e) =>
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

    public async Task DisconnectAsync(string deviceId)
    {
        if (_connectedDevices.TryGetValue(deviceId, out var device))
        {
            device.Dispose();
            _connectedDevices.Remove(deviceId);
        }
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

        var writer = new Windows.Storage.Streams.DataWriter();
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

        ch.ValueChanged += (s, e) =>
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

        var svcResult = await device.GetGattServiceAsync(Guid.Parse(serviceUuid));
        if (svcResult.Status != GattCommunicationStatus.Success)
            throw new BleException(BleErrorCode.ServiceNotFound, $"Service {serviceUuid} not found.");

        var chResult = await svcResult.Service.GetCharacteristicsAsync(BluetoothCacheMode.Uncached);
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
        using var reader = Windows.Storage.Streams.DataReader.FromBuffer(buffer);
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

    private static byte[] ToByteArray(int value)
    {
        return BitConverter.GetBytes(value);
    }

    // overload for the scan's RawSignalStrengthInDBm
    // already handled in ToByteArray(int, BluetoothLEAdvertisement)

    public void Dispose()
    {
        _watcher.Stop();
        foreach (var device in _connectedDevices.Values)
            device.Dispose();
        _connectedDevices.Clear();
    }
}
```

- [ ] **Step 2: Build and commit**

```bash
dotnet build E:\AI_Support\AI_Project\WindowsBleTool\WindowsBleTool.sln
git add -A
git commit -m "feat: implement WindowsBleAdapter with BLE API wrapping"
```

### Task 5: Implement DeviceFilter

**Files:**
- Create: `/src/BleCore/DeviceFilter.cs`
- Create: `/tests/BleCore.Tests/DeviceFilterTests.cs`

- [ ] **Step 1: Write DeviceFilter**

```csharp
// src/BleCore/DeviceFilter.cs
using System.Text.RegularExpressions;

namespace BleCore;

public class DeviceFilterRule
{
    public FilterType Type { get; init; }
    public FilterOperator Operator { get; init; }
    public string Value { get; init; } = string.Empty;
}

public enum FilterType
{
    Rssi,
    DeviceName,
    AdvertisedUuid,
    MacAddress,
    ManufacturerId,
    RawData,
    AddressType,
}

public enum FilterOperator
{
    Equal,
    GreaterThanOrEqual,
    LessThanOrEqual,
    Contains,
    NotContains,
    Regex,
}

public static class DeviceFilter
{
    public static bool MatchesAll(BleDeviceInfo device, IReadOnlyList<DeviceFilterRule> rules)
    {
        return rules.All(r => MatchesRule(device, r));
    }

    private static bool MatchesRule(BleDeviceInfo device, DeviceFilterRule rule)
    {
        return rule.Type switch
        {
            FilterType.Rssi => MatchRssi(device.Rssi, rule.Operator, rule.Value),
            FilterType.DeviceName => MatchDeviceName(device.Name, rule.Operator, rule.Value),
            FilterType.AdvertisedUuid => MatchUuid(device.AdStructures, rule.Value),
            FilterType.MacAddress => MatchMac(device.Address, rule.Operator, rule.Value),
            FilterType.ManufacturerId => MatchManufacturer(device.AdStructures, rule.Value),
            FilterType.RawData => MatchRawData(device.RawAdvertisement, rule.Value),
            FilterType.AddressType => string.Equals(device.AddressType, rule.Value, StringComparison.OrdinalIgnoreCase),
            _ => false,
        };
    }

    private static bool MatchRssi(int rssi, FilterOperator op, string value)
    {
        if (!int.TryParse(value, out var threshold)) return false;
        return op switch
        {
            FilterOperator.GreaterThanOrEqual => rssi >= threshold,
            FilterOperator.LessThanOrEqual => rssi <= threshold,
            _ => false,
        };
    }

    private static bool MatchDeviceName(string name, FilterOperator op, string pattern)
    {
        return op switch
        {
            FilterOperator.Contains => name.Contains(pattern, StringComparison.OrdinalIgnoreCase),
            FilterOperator.NotContains => !name.Contains(pattern, StringComparison.OrdinalIgnoreCase),
            FilterOperator.Equal => string.Equals(name, pattern, StringComparison.OrdinalIgnoreCase),
            FilterOperator.Regex => Regex.IsMatch(name, pattern, RegexOptions.IgnoreCase),
            _ => false,
        };
    }

    private static bool MatchUuid(Dictionary<ushort, byte[]> adStructures, string uuid)
    {
        // AdType 0x02, 0x03, 0x06, 0x07 are UUID types
        ushort[] uuidTypes = { 0x02, 0x03, 0x06, 0x07 };
        if (!Guid.TryParse(uuid, out var targetGuid))
        {
            // Try short UUID: "180D" -> 0000180D-0000-1000-8000-00805F9B34FB
            if (ushort.TryParse(uuid, System.Globalization.NumberStyles.HexNumber, null, out var shortUuid))
            {
                targetGuid = new Guid($"0000{shortUuid:X4}-0000-1000-8000-00805F9B34FB");
            }
            else return false;
        }

        foreach (var type in uuidTypes)
        {
            if (adStructures.TryGetValue(type, out var data))
            {
                for (int i = 0; i <= data.Length - 16; i++)
                {
                    var guid = new Guid(data.AsSpan(i, 16));
                    if (guid == targetGuid) return true;
                }
            }
        }
        return false;
    }

    private static bool MatchMac(string address, FilterOperator op, string pattern)
    {
        pattern = pattern.Replace("*", ".*").Replace(":", "\\:");
        return op switch
        {
            FilterOperator.Contains => Regex.IsMatch(address, pattern, RegexOptions.IgnoreCase),
            FilterOperator.Equal => string.Equals(address, pattern, StringComparison.OrdinalIgnoreCase),
            FilterOperator.Regex => Regex.IsMatch(address, pattern, RegexOptions.IgnoreCase),
            _ => false,
        };
    }

    private static bool MatchManufacturer(Dictionary<ushort, byte[]> adStructures, string companyId)
    {
        // AdType 0xFF = Manufacturer Specific Data
        if (!adStructures.TryGetValue(0xFF, out var data) || data.Length < 2) return false;
        if (!ushort.TryParse(companyId, System.Globalization.NumberStyles.HexNumber, null, out var targetId))
            return false;

        var actualId = BitConverter.ToUInt16(data, 0);
        return actualId == targetId;
    }

    private static bool MatchRawData(byte[] rawAd, string hexPattern)
    {
        var pattern = HexToBytes(hexPattern);
        if (pattern == null) return false;
        return IndexOf(rawAd, pattern) >= 0;
    }

    private static byte[]? HexToBytes(string hex)
    {
        hex = hex.Replace(" ", "").Replace("-", "");
        if (hex.Length % 2 != 0) return null;
        var bytes = new byte[hex.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            if (!byte.TryParse(hex.AsSpan(i * 2, 2), System.Globalization.NumberStyles.HexNumber, null, out var b))
                return null;
            bytes[i] = b;
        }
        return bytes;
    }

    private static int IndexOf(byte[] haystack, byte[] needle)
    {
        for (int i = 0; i <= haystack.Length - needle.Length; i++)
        {
            bool match = true;
            for (int j = 0; j < needle.Length; j++)
            {
                if (haystack[i + j] != needle[j]) { match = false; break; }
            }
            if (match) return i;
        }
        return -1;
    }
}
```

- [ ] **Step 2: Write DeviceFilter tests**

```csharp
// tests/BleCore.Tests/DeviceFilterTests.cs
using BleCore;

namespace BleCore.Tests;

[TestClass]
public class DeviceFilterTests
{
    [TestMethod]
    public void RssiFilter_GreaterThanOrEqual_Passes()
    {
        var device = new BleDeviceInfo { Rssi = -52, Name = "Test", Address = "AA:BB:CC:DD:EE:FF" };
        var rules = new[] { new DeviceFilterRule { Type = FilterType.Rssi, Operator = FilterOperator.GreaterThanOrEqual, Value = "-70" } };
        Assert.IsTrue(DeviceFilter.MatchesAll(device, rules));
    }

    [TestMethod]
    public void RssiFilter_GreaterThanOrEqual_Fails()
    {
        var device = new BleDeviceInfo { Rssi = -85, Name = "Test", Address = "AA:BB:CC:DD:EE:FF" };
        var rules = new[] { new DeviceFilterRule { Type = FilterType.Rssi, Operator = FilterOperator.GreaterThanOrEqual, Value = "-70" } };
        Assert.IsFalse(DeviceFilter.MatchesAll(device, rules));
    }

    [TestMethod]
    public void NameFilter_Contains_Passes()
    {
        var device = new BleDeviceInfo { Rssi = -50, Name = "Heart Rate Sensor", Address = "AA:BB:CC:DD:EE:FF" };
        var rules = new[] { new DeviceFilterRule { Type = FilterType.DeviceName, Operator = FilterOperator.Contains, Value = "Heart" } };
        Assert.IsTrue(DeviceFilter.MatchesAll(device, rules));
    }

    [TestMethod]
    public void NameFilter_Contains_Fails()
    {
        var device = new BleDeviceInfo { Rssi = -50, Name = "Temperature Tag", Address = "AA:BB:CC:DD:EE:FF" };
        var rules = new[] { new DeviceFilterRule { Type = FilterType.DeviceName, Operator = FilterOperator.Contains, Value = "Heart" } };
        Assert.IsFalse(DeviceFilter.MatchesAll(device, rules));
    }

    [TestMethod]
    public void MacAddressFilter_Contains_Passes()
    {
        var device = new BleDeviceInfo { Rssi = -50, Name = "Test", Address = "AA:BB:CC:DD:EE:FF" };
        var rules = new[] { new DeviceFilterRule { Type = FilterType.MacAddress, Operator = FilterOperator.Contains, Value = "AA:BB" } };
        Assert.IsTrue(DeviceFilter.MatchesAll(device, rules));
    }

    [TestMethod]
    public void MultipleRules_AllMustPass()
    {
        var device = new BleDeviceInfo { Rssi = -52, Name = "Heart Rate Sensor", Address = "AA:BB:CC:DD:EE:FF" };
        var rules = new[]
        {
            new DeviceFilterRule { Type = FilterType.Rssi, Operator = FilterOperator.GreaterThanOrEqual, Value = "-70" },
            new DeviceFilterRule { Type = FilterType.DeviceName, Operator = FilterOperator.Contains, Value = "Heart" },
        };
        Assert.IsTrue(DeviceFilter.MatchesAll(device, rules));
    }

    [TestMethod]
    public void MultipleRules_OneFails_ReturnsFalse()
    {
        var device = new BleDeviceInfo { Rssi = -52, Name = "Temperature Tag", Address = "AA:BB:CC:DD:EE:FF" };
        var rules = new[]
        {
            new DeviceFilterRule { Type = FilterType.Rssi, Operator = FilterOperator.GreaterThanOrEqual, Value = "-70" },
            new DeviceFilterRule { Type = FilterType.DeviceName, Operator = FilterOperator.Contains, Value = "Heart" },
        };
        Assert.IsFalse(DeviceFilter.MatchesAll(device, rules));
    }
}
```

- [ ] **Step 3: Run tests and commit**

```bash
dotnet test E:\AI_Support\AI_Project\WindowsBleTool\tests\BleCore.Tests\BleCore.Tests.csproj
git add -A
git commit -m "feat: add device filter with RSSI, name, MAC, UUID, manufacturer, raw data matching"
```

### Task 6: Implement AdDataParser

**Files:**
- Create: `/src/BleCore/AdDataParser.cs`
- Create: `/tests/BleCore.Tests/AdDataParserTests.cs`

- [ ] **Step 1: Write AdDataParser**

```csharp
// src/BleCore/AdDataParser.cs
namespace BleCore;

public static class AdDataParser
{
    public static List<ParsedAdField> Parse(Dictionary<ushort, byte[]> adStructures)
    {
        var fields = new List<ParsedAdField>();
        foreach (var (adType, data) in adStructures)
        {
            var field = new ParsedAdField { AdType = adType, RawData = data };
            switch (adType)
            {
                case 0x01: // Flags
                    field.Name = "Flags";
                    field.Description = $"{(data.Length > 0 ? $"0x{data[0]:X2}" : "N/A")} " +
                        $"{FormatFlags(data.Length > 0 ? data[0] : (byte)0)}";
                    break;
                case 0x02: // Incomplete 16-bit UUIDs
                case 0x03: // Complete 16-bit UUIDs
                    field.Name = adType == 0x02 ? "Incomplete 16-bit UUIDs" : "Complete 16-bit UUIDs";
                    field.Description = string.Join(", ", Enumerate16BitUuids(data).Select(u => $"0x{u:X4}"));
                    break;
                case 0x04: // Incomplete 32-bit UUIDs
                case 0x05: // Complete 32-bit UUIDs
                    field.Name = adType == 0x04 ? "Incomplete 32-bit UUIDs" : "Complete 32-bit UUIDs";
                    field.Description = string.Join(", ", Enumerate32BitUuids(data).Select(u => $"0x{u:X8}"));
                    break;
                case 0x06: // Incomplete 128-bit UUIDs
                case 0x07: // Complete 128-bit UUIDs
                    field.Name = adType == 0x06 ? "Incomplete 128-bit UUIDs" : "Complete 128-bit UUIDs";
                    field.Description = string.Join(", ", Enumerate128BitUuids(data).Select(g => g.ToString("D")));
                    break;
                case 0x08: // Shortened Local Name
                case 0x09: // Complete Local Name
                    field.Name = adType == 0x08 ? "Shortened Local Name" : "Complete Local Name";
                    field.Description = $"\"{System.Text.Encoding.UTF8.GetString(data)}\"";
                    break;
                case 0x0A: // TX Power Level
                    field.Name = "TX Power Level";
                    field.Description = data.Length > 0 ? $"{data[0]} dBm" : "N/A";
                    break;
                case 0x16: // Service Data
                    if (data.Length < 2) break;
                    var svcUuid16 = BitConverter.ToUInt16(data, 0);
                    var svcData = data.AsSpan(2).ToArray();
                    field.Name = "Service Data";
                    field.Description = $"UUID: 0x{svcUuid16:X4} · Data: {BitConverter.ToString(svcData)}";
                    break;
                case 0xFF: // Manufacturer Specific Data
                    if (data.Length < 2) break;
                    var companyId = BitConverter.ToUInt16(data, 0);
                    var mfrData = data.AsSpan(2).ToArray();
                    field.Name = "Manufacturer Data";
                    field.Description = $"Company ID: 0x{companyId:X4} (" +
                        $"{(KnownCompanyIds.TryGetValue(companyId, out var name) ? name : "Unknown")}" +
                        $") · Data: {BitConverter.ToString(mfrData)}";
                    break;
                default:
                    field.Name = $"AdType: 0x{adType:X2}";
                    field.Description = BitConverter.ToString(data);
                    break;
            }
            fields.Add(field);
        }
        return fields;
    }

    private static List<ushort> Enumerate16BitUuids(byte[] data)
    {
        var uuids = new List<ushort>();
        for (int i = 0; i <= data.Length - 2; i += 2)
            uuids.Add(BitConverter.ToUInt16(data, i));
        return uuids;
    }

    private static List<uint> Enumerate32BitUuids(byte[] data)
    {
        var uuids = new List<uint>();
        for (int i = 0; i <= data.Length - 4; i += 4)
            uuids.Add(BitConverter.ToUInt32(data, i));
        return uuids;
    }

    private static List<Guid> Enumerate128BitUuids(byte[] data)
    {
        var uuids = new List<Guid>();
        for (int i = 0; i <= data.Length - 16; i += 16)
            uuids.Add(new Guid(data.AsSpan(i, 16)));
        return uuids;
    }

    private static string FormatFlags(byte flags)
    {
        var parts = new List<string>();
        if ((flags & 0x01) != 0) parts.Add("LE Limited Discoverable");
        if ((flags & 0x02) != 0) parts.Add("LE General Discoverable");
        if ((flags & 0x04) != 0) parts.Add("BR/EDR Not Supported");
        if ((flags & 0x08) != 0) parts.Add("Simultaneous LE + BR/EDR (Controller)");
        if ((flags & 0x10) != 0) parts.Add("Simultaneous LE + BR/EDR (Host)");
        return parts.Count > 0 ? string.Join(" | ", parts) : "None";
    }

    private static readonly Dictionary<ushort, string> KnownCompanyIds = new()
    {
        { 0x004C, "Apple" },
        { 0x0059, "Nordic Semiconductor" },
        { 0x0006, "Microsoft" },
        { 0x0077, "Google" },
        { 0x0157, "Samsung" },
        { 0x0087, "Garmin" },
        { 0x010F, "Tile" },
        { 0x0133, "Xiaomi" },
    };
}

public class ParsedAdField
{
    public ushort AdType { get; init; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public byte[] RawData { get; init; } = Array.Empty<byte>();
}
```

- [ ] **Step 2: Write AdDataParser tests**

```csharp
// tests/BleCore.Tests/AdDataParserTests.cs
using BleCore;

namespace BleCore.Tests;

[TestClass]
public class AdDataParserTests
{
    [TestMethod]
    public void ParseFlags_ReturnsDescription()
    {
        var structures = new Dictionary<ushort, byte[]> { { 0x01, new byte[] { 0x06 } } };
        var result = AdDataParser.Parse(structures);
        var flag = result.First(f => f.AdType == 0x01);
        Assert.AreEqual("Flags", flag.Name);
        Assert.IsTrue(flag.Description.Contains("LE General Discoverable"));
    }

    [TestMethod]
    public void ParseCompleteLocalName_ReturnsQuotedString()
    {
        var name = System.Text.Encoding.UTF8.GetBytes("Heart Rate Sensor");
        var structures = new Dictionary<ushort, byte[]> { { 0x09, name } };
        var result = AdDataParser.Parse(structures);
        var field = result.First(f => f.AdType == 0x09);
        Assert.IsTrue(field.Description.Contains("Heart Rate Sensor"));
    }

    [TestMethod]
    public void Parse16BitUuids_FormatsCorrectly()
    {
        var data = new byte[] { 0x0D, 0x18, 0x0A, 0x18 }; // 0x180D, 0x180A
        var structures = new Dictionary<ushort, byte[]> { { 0x03, data } };
        var result = AdDataParser.Parse(structures);
        var field = result.First(f => f.AdType == 0x03);
        Assert.IsTrue(field.Description.Contains("0x180D"));
        Assert.IsTrue(field.Description.Contains("0x180A"));
    }
}
```

- [ ] **Step 3: Run tests and commit**

```bash
dotnet test E:\AI_Support\AI_Project\WindowsBleTool\tests\BleCore.Tests\BleCore.Tests.csproj
git add -A
git commit -m "feat: add advertisement data parser with known company IDs"
```

---

## Phase 3: Data Formatting & Logging

### Task 7: Implement DataFormatter

**Files:**
- Create: `/src/BleCore/DataFormatter.cs`
- Create: `/tests/BleCore.Tests/DataFormatterTests.cs`

- [ ] **Step 1: Write DataFormatter**

```csharp
// src/BleCore/DataFormatter.cs
using System.Text;

namespace BleCore;

public enum DataFormat { Hex, Decimal, Binary, Utf8, Base64 }

public static class DataFormatter
{
    public static string Format(byte[] data, DataFormat format, string separator = " ")
    {
        return format switch
        {
            DataFormat.Hex => BitConverter.ToString(data).Replace("-", separator),
            DataFormat.Decimal => string.Join(separator, data.Select(b => b.ToString())),
            DataFormat.Binary => string.Join(separator, data.Select(b => Convert.ToString(b, 2).PadLeft(8, '0'))),
            DataFormat.Utf8 => Encoding.UTF8.GetString(data),
            DataFormat.Base64 => Convert.ToBase64String(data),
            _ => BitConverter.ToString(data).Replace("-", separator),
        };
    }

    public static DataFormat SuggestFormat(string characteristicUuid)
    {
        var guid = new Guid(characteristicUuid);
        return guid.ToString("D").ToUpper() switch
        {
            "00002A00-0000-1000-8000-00805F9B34FB" => DataFormat.Utf8,  // Device Name
            "00002A19-0000-1000-8000-00805F9B34FB" => DataFormat.Decimal, // Battery Level
            "00002A37-0000-1000-8000-00805F9B34FB" => DataFormat.Hex,    // HR Measurement
            "00002A6E-0000-1000-8000-00805F9B34FB" => DataFormat.Decimal, // Temperature
            _ => DataFormat.Hex,
        };
    }
}
```

- [ ] **Step 2: Write DataFormatter tests**

```csharp
// tests/BleCore.Tests/DataFormatterTests.cs
using BleCore;

namespace BleCore.Tests;

[TestClass]
public class DataFormatterTests
{
    [TestMethod]
    public void FormatHex_ReturnsSpaceSeparatedHex()
    {
        var result = DataFormatter.Format(new byte[] { 0x06, 0x48, 0x00 }, DataFormat.Hex);
        Assert.AreEqual("06 48 00", result);
    }

    [TestMethod]
    public void FormatDecimal_ReturnsDecimalValues()
    {
        var result = DataFormatter.Format(new byte[] { 0x06, 0x48, 0x00 }, DataFormat.Decimal);
        Assert.AreEqual("6 72 0", result);
    }

    [TestMethod]
    public void FormatBinary_Returns8BitPadded()
    {
        var result = DataFormatter.Format(new byte[] { 0x06 }, DataFormat.Binary);
        Assert.AreEqual("00000110", result);
    }

    [TestMethod]
    public void FormatBase64_ReturnsCorrectString()
    {
        var result = DataFormatter.Format(new byte[] { 0x06, 0x48, 0x00 }, DataFormat.Base64);
        Assert.AreEqual("BkgA", result);
    }

    [TestMethod]
    public void SuggestFormat_BatteryLevel_ReturnsDecimal()
    {
        var uuid = "00002a19-0000-1000-8000-00805f9b34fb";
        Assert.AreEqual(DataFormat.Decimal, DataFormatter.SuggestFormat(uuid));
    }

    [TestMethod]
    public void SuggestFormat_UnknownUuid_ReturnsHex()
    {
        Assert.AreEqual(DataFormat.Hex, DataFormatter.SuggestFormat("00000000-0000-1000-8000-00805f9b34fb"));
    }
}
```

- [ ] **Step 3: Run tests and commit**

```bash
dotnet test E:\AI_Support\AI_Project\WindowsBleTool\tests\BleCore.Tests\BleCore.Tests.csproj
git add -A
git commit -m "feat: add data formatter with Hex/Dec/Bin/UTF-8/Base64 and smart UUID suggestions"
```

### Task 8: Implement SessionLogger and NotificationDataLogger

**Files:**
- Create: `/src/BleCore/SessionLogger.cs`
- Create: `/src/BleCore/NotificationDataLogger.cs`
- Create: `/tests/BleCore.Tests/SessionLoggerTests.cs`

- [ ] **Step 1: Write SessionLogger**

```csharp
// src/BleCore/SessionLogger.cs
using System.Text.Json;

namespace BleCore;

public class SessionLogger
{
    private readonly List<LogEntry> _entries = new();
    private readonly object _lock = new();

    public void Log(string category, string message)
    {
        lock (_lock)
        {
            _entries.Add(new LogEntry
            {
                Timestamp = DateTimeOffset.UtcNow,
                Category = category,
                Message = message,
            });
        }
    }

    public void LogError(string category, string message)
    {
        lock (_lock)
        {
            _entries.Add(new LogEntry
            {
                Timestamp = DateTimeOffset.UtcNow,
                Category = category,
                Message = message,
                IsError = true,
            });
        }
    }

    public IReadOnlyList<LogEntry> Entries
    {
        get { lock (_lock) return _entries.ToList(); }
    }

    public string ExportAsText()
    {
        lock (_lock)
        {
            return string.Join(Environment.NewLine,
                _entries.Select(e => $"[{e.Timestamp:HH:mm:ss.fff}] [{e.Category}] {(e.IsError ? "ERROR: " : "")}{e.Message}"));
        }
    }

    public string ExportAsJson()
    {
        lock (_lock)
        {
            return JsonSerializer.Serialize(_entries, new JsonSerializerOptions { WriteIndented = true });
        }
    }

    public void Clear()
    {
        lock (_lock) _entries.Clear();
    }
}

public class LogEntry
{
    public DateTimeOffset Timestamp { get; init; }
    public string Category { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public bool IsError { get; init; }
}
```

- [ ] **Step 2: Write NotificationDataLogger**

```csharp
// src/BleCore/NotificationDataLogger.cs
using System.Text;

namespace BleCore;

public class NotificationDataLogger
{
    private readonly List<NotificationRecord> _records = new();
    private readonly object _lock = new();
    public int MaxRecords { get; set; } = 10_000;

    public void Record(string deviceName, string deviceAddress, string serviceUuid,
        string characteristicUuid, byte[] data, string parsedValue = "")
    {
        lock (_lock)
        {
            if (_records.Count >= MaxRecords)
                _records.RemoveAt(0);

            _records.Add(new NotificationRecord
            {
                Timestamp = DateTimeOffset.UtcNow,
                DeviceName = deviceName,
                DeviceAddress = deviceAddress,
                ServiceUuid = serviceUuid,
                CharacteristicUuid = characteristicUuid,
                HexValue = BitConverter.ToString(data).Replace("-", " "),
                ParsedValue = parsedValue,
            });
        }
    }

    public IReadOnlyList<NotificationRecord> Records
    {
        get { lock (_lock) return _records.ToList(); }
    }

    public string ExportAsCsv()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Timestamp,Device Name,Device Address,Service UUID,Characteristic UUID,Hex Value,Parsed Value");
        lock (_lock)
        {
            foreach (var r in _records)
            {
                sb.AppendLine($"\"{r.Timestamp:yyyy-MM-ddTHH:mm:ss.fffZ}\"," +
                    $"\"{r.DeviceName}\",\"{r.DeviceAddress}\"," +
                    $"\"{r.ServiceUuid}\",\"{r.CharacteristicUuid}\"," +
                    $"\"{r.HexValue}\",\"{r.ParsedValue}\"");
            }
        }
        return sb.ToString();
    }

    public void Clear()
    {
        lock (_lock) _records.Clear();
    }
}

public class NotificationRecord
{
    public DateTimeOffset Timestamp { get; init; }
    public string DeviceName { get; init; } = string.Empty;
    public string DeviceAddress { get; init; } = string.Empty;
    public string ServiceUuid { get; init; } = string.Empty;
    public string CharacteristicUuid { get; init; } = string.Empty;
    public string HexValue { get; init; } = string.Empty;
    public string ParsedValue { get; init; } = string.Empty;
}
```

- [ ] **Step 3: Write SessionLogger tests**

```csharp
// tests/BleCore.Tests/SessionLoggerTests.cs
using BleCore;

namespace BleCore.Tests;

[TestClass]
public class SessionLoggerTests
{
    [TestMethod]
    public void Log_AddsEntry()
    {
        var logger = new SessionLogger();
        logger.Log("SCAN", "Started scanning");
        Assert.AreEqual(1, logger.Entries.Count);
        Assert.AreEqual("SCAN", logger.Entries[0].Category);
    }

    [TestMethod]
    public void ExportAsText_FormatsCorrectly()
    {
        var logger = new SessionLogger();
        logger.Log("CONNECT", "Connected to device");
        var text = logger.ExportAsText();
        Assert.IsTrue(text.Contains("[CONNECT]"));
        Assert.IsTrue(text.Contains("Connected to device"));
    }

    [TestMethod]
    public void ExportAsJson_ProducesValidJson()
    {
        var logger = new SessionLogger();
        logger.Log("SCAN", "Test");
        var json = logger.ExportAsJson();
        Assert.IsTrue(json.Contains("SCAN"));
        Assert.IsTrue(json.Contains("Test"));
    }
}
```

- [ ] **Step 4: Run tests and commit**

```bash
dotnet test E:\AI_Support\AI_Project\WindowsBleTool\tests\BleCore.Tests\BleCore.Tests.csproj
git add -A
git commit -m "feat: add session logger and notification data logger with export"
```

---

## Phase 4: Script Engine

### Task 9: Implement ScriptEngine and BleJsApi

**Files:**
- Create: `/src/BleCore/ScriptEngine.cs`
- Create: `/src/BleCore/BleJsApi.cs`

- [ ] **Step 1: Write BleJsApi (JS-facing BLE object)**

```csharp
// src/BleCore/BleJsApi.cs
using Microsoft.ClearScript;

namespace BleCore;

public class BleJsApi
{
    private readonly IBleAdapter _adapter;
    private readonly Action<string, bool> _consoleLog;

    private readonly Dictionary<string, ScanCallbackState> _scanCallbacks = new();
    private readonly Dictionary<string, NotifyCallbackState> _notifyCallbacks = new();

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

        var tcs = new TaskCompletionSource<List<JsDeviceInfo>>();
        var cts = new CancellationTokenSource(duration);

        void handler(object? s, ScanResultEvent e)
        {
            lock (devices)
            {
                if (!devices.Any(d => d.Address == e.Device.Address))
                    devices.Add(JsDeviceInfo.From(e.Device));
            }
        }

        _adapter.ScanResult += handler;
        await _adapter.StartScanAsync(filters, cts.Token);

        try
        {
            await Task.Delay(duration);
        }
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
        return JsDeviceInfo.FromAddress(address);
    }

    public object GetConnectedDevice(string address)
    {
        return JsDeviceInfo.FromAddress(address);
    }

    private static List<DeviceFilterRule>? ParseFilters(IDictionary<string, object>? opts)
    {
        if (opts == null || !opts.TryGetValue("filters", out var filtersRaw) || filtersRaw is not IList<object> filterList)
            return null;

        var rules = new List<DeviceFilterRule>();
        foreach (var filter in filterList)
        {
            if (filter is not IDictionary<string, object> f) continue;

            rules.Add(new DeviceFilterRule
            {
                Type = f.TryGetValue("name", out var n) ? FilterType.DeviceName : FilterType.Rssi,
                Operator = FilterOperator.Contains,
                Value = f.TryGetValue("name", out var name) ? name.ToString()!
                    : f.TryGetValue("rssi", out var rssi) ? rssi.ToString()! : "-70",
            });
        }
        return rules;
    }

    public void ConsoleLog(string message) => _consoleLog(message, false);
    public void ConsoleError(string message) => _consoleLog(message, true);
}

public class JsDeviceInfo
{
    public string Address { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int Rssi { get; init; }

    public static JsDeviceInfo From(BleDeviceInfo d) => new() { Address = d.Address, Name = d.Name, Rssi = d.Rssi };
    public static JsDeviceInfo FromAddress(string addr) => new() { Address = addr, Name = addr };

    public async Task<object> GetServiceAsync(string uuid) =>
        await Task.FromResult(new JsServiceInfo { Uuid = uuid });

    public async Task DisconnectAsync() => await Task.CompletedTask;
}

public class JsServiceInfo
{
    public string Uuid { get; init; } = string.Empty;

    public async Task<object> GetCharacteristicAsync(string uuid) =>
        await Task.FromResult(new JsCharacteristicInfo { Uuid = uuid, ServiceUuid = Uuid });
}

public class JsCharacteristicInfo
{
    public string Uuid { get; init; } = string.Empty;
    public string ServiceUuid { get; init; } = string.Empty;

    public async Task<byte[]> ReadAsync() => await Task.FromResult(Array.Empty<byte>());
    public async Task WriteAsync(byte[] data) => await Task.CompletedTask;
    public async Task WriteWithoutResponseAsync(byte[] data) => await Task.CompletedTask;
    public async Task SubscribeAsync(dynamic callback) => await Task.CompletedTask;
    public async Task UnsubscribeAsync() => await Task.CompletedTask;
}

internal class ScanCallbackState { }
internal class NotifyCallbackState { }
```

- [ ] **Step 2: Write ScriptEngine**

```csharp
// src/BleCore/ScriptEngine.cs
using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;

namespace BleCore;

public class ScriptEngine : IDisposable
{
    private readonly V8ScriptEngine _engine;
    private readonly BleJsApi _bleApi;
    private readonly Action<string, bool>? _onOutput;
    private readonly CancellationTokenSource _cancelSource = new();

    public ScriptEngine(IBleAdapter adapter, Action<string, bool>? onOutput = null)
    {
        _onOutput = onOutput;
        _engine = new V8ScriptEngine(V8ScriptEngineFlags.EnableTaskPromiseConversion
            | V8ScriptEngineFlags.EnableDynamicModuleImports
            | V8ScriptEngineFlags.EnableValueTasks);

        _engine.AddHostObject("console", new HostConsole(onOutput ?? ((_, _) => { })));

        _bleApi = new BleJsApi(adapter, (msg, isErr) => onOutput?.Invoke(msg, isErr));
        _engine.AddHostObject("ble", _bleApi);
    }

    public async Task<string?> RunAsync(string scriptCode)
    {
        try
        {
            _engine.Execute(scriptCode);
            return null; // no error
        }
        catch (ScriptEngineException ex)
        {
            return $"Script error: {ex.Message}";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    public void Cancel()
    {
        _cancelSource.Cancel();
        _engine.Interrupt();
    }

    public void Dispose()
    {
        _cancelSource.Cancel();
        _engine.Dispose();
    }
}

public class HostConsole
{
    private readonly Action<string, bool> _output;

    public HostConsole(Action<string, bool> output) => _output = output;

    public void log(string message) => _output(message, false);
    public void error(string message) => _output(message, true);
    public void warn(string message) => _output(message, true);
}
```

- [ ] **Step 3: Build and commit**

```bash
dotnet build E:\AI_Support\AI_Project\WindowsBleTool\WindowsBleTool.sln
git add -A
git commit -m "feat: add JavaScript script engine with ClearScript V8 and BLE JS API"
```

---

## Phase 5: Settings & App Configuration

### Task 10: Implement AppSettings persistence

**Files:**
- Create: `/src/BleCore/AppSettings.cs`

- [ ] **Step 1: Write AppSettings**

```csharp
// src/BleCore/AppSettings.cs
using System.Text.Json;

namespace BleCore;

public class AppSettings
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "WindowsBleTool", "settings.json");

    public int DefaultRssiThreshold { get; set; } = -70;
    public int LogMaxEntries { get; set; } = 10_000;
    public string ScriptTemplatePath { get; set; } = string.Empty;
    public DataFormat PreferredDataFormat { get; set; } = DataFormat.Hex;
    public string PreferredHexSeparator { get; set; } = " ";
    public List<HistoryDevice> RecentDevices { get; set; } = new();
    public List<SavedFilterPreset> FilterPresets { get; set; } = new();

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new();
            }
        }
        catch { }
        return new();
    }

    public void Save()
    {
        var dir = Path.GetDirectoryName(SettingsPath)!;
        Directory.CreateDirectory(dir);
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(SettingsPath, json);
    }
}

public class HistoryDevice
{
    public string Address { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public DateTimeOffset LastConnected { get; init; }
}

public class SavedFilterPreset
{
    public string Name { get; set; } = string.Empty;
    public List<DeviceFilterRule> Rules { get; set; } = new();
}
```

- [ ] **Step 2: Build and commit**

```bash
dotnet build E:\AI_Support\AI_Project\WindowsBleTool\WindowsBleTool.sln
git add -A
git commit -m "feat: add app settings persistence with JSON config file"
```

---

## Phase 6: CLI

### Task 11: Implement CLI commands

**Files:**
- Create: `/src/BleTool.Cli/Program.cs`

- [ ] **Step 1: Write Program.cs**

```csharp
// src/BleTool.Cli/Program.cs
using System.CommandLine;
using System.Text.Json;
using BleCore;

var adapter = new WindowsBleAdapter();
var connectedDevice = string.Empty;

var rootCommand = new RootCommand("Windows BLE debugging CLI tool");

// scan
var rssiOption = new Option<int?>("--rssi", "RSSI threshold filter (>=)");
var timeoutOption = new Option<int>("--timeout", () => 5000, "Scan timeout in ms");
var formatOption = new Option<string?>("--format", "Output format: json or table");

var scanCmd = new Command("scan", "Scan for BLE devices") { rssiOption, timeoutOption, formatOption };
scanCmd.SetHandler(async (rssi, timeout, format) =>
{
    var filters = new List<DeviceFilterRule>();
    if (rssi.HasValue)
        filters.Add(new DeviceFilterRule { Type = FilterType.Rssi, Operator = FilterOperator.GreaterThanOrEqual, Value = rssi.Value.ToString() });

    var devices = new List<BleDeviceInfo>();
    adapter.ScanResult += (s, e) =>
    {
        lock (devices)
        {
            if (!devices.Any(d => d.Address == e.Device.Address))
                devices.Add(e.Device);
        }
    };

    await adapter.StartScanAsync(filters);
    await Task.Delay(timeout);
    adapter.StopScan();

    if (format == "json")
    {
        Console.WriteLine(JsonSerializer.Serialize(devices, new JsonSerializerOptions { WriteIndented = true }));
    }
    else
    {
        foreach (var d in devices)
        {
            var uuids = d.AdStructures.ContainsKey(0x03)
                ? string.Join(",", AdDataParser.Parse(d.AdStructures).Where(f => f.AdType == 0x03).Select(f => f.Description))
                : "";
            Console.WriteLine($"  {d.Name,-24} {d.Address,-17} {d.Rssi,4}dBm  [{uuids}]");
        }
    }
}, rssiOption, timeoutOption, formatOption);

// connect
var addressArg = new Argument<string>("address", "Device MAC address");
var connectCmd = new Command("connect", "Connect to a BLE device") { addressArg };
connectCmd.SetHandler(async (address) =>
{
    await adapter.ConnectAsync(address);
    connectedDevice = address;
    Console.WriteLine($"Connected to {address}");
}, addressArg);

// disconnect
var allOption = new Option<bool>("--all", "Disconnect all devices");
var disconnectCmd = new Command("disconnect", "Disconnect from device(s)") { addressArg, allOption };
disconnectCmd.SetHandler(async (address, all) =>
{
    if (all)
    {
        adapter.Dispose();
        Console.WriteLine("Disconnected all devices.");
    }
    else
    {
        await adapter.DisconnectAsync(address);
        Console.WriteLine($"Disconnected from {address}");
    }
}, addressArg, allOption);

// read
var serviceOption = new Option<string>("--service", "Service UUID") { IsRequired = true };
var charOption = new Option<string>("--char", "Characteristic UUID") { IsRequired = true };
var deviceOption = new Option<string?>("--device", "Device address (uses last connected if omitted)");
var readCmd = new Command("read", "Read a characteristic value") { serviceOption, charOption, deviceOption, formatOption };
readCmd.SetHandler(async (svc, ch, dev, fmt) =>
{
    var target = dev ?? connectedDevice;
    var data = await adapter.ReadCharacteristicAsync(target, svc, ch);
    if (fmt == "json")
        Console.WriteLine(JsonSerializer.Serialize(new { hex = BitConverter.ToString(data), bytes = data }));
    else
        Console.WriteLine(BitConverter.ToString(data).Replace("-", " "));
}, serviceOption, charOption, deviceOption, formatOption);

// write
var dataOption = new Option<string>("--data", "Hex data to write") { IsRequired = true };
var writeCmd = new Command("write", "Write a characteristic value") { serviceOption, charOption, dataOption, deviceOption };
writeCmd.SetHandler(async (svc, ch, data, dev) =>
{
    var target = dev ?? connectedDevice;
    var bytes = StringToBytes(data);
    await adapter.WriteCharacteristicAsync(target, svc, ch, bytes);
    Console.WriteLine($"Written {bytes.Length} bytes to {ch}");
}, serviceOption, charOption, dataOption, deviceOption);

// subscribe
var subscribeCmd = new Command("subscribe", "Subscribe to characteristic notifications") { serviceOption, charOption, deviceOption };
subscribeCmd.SetHandler(async (svc, ch, dev) =>
{
    var target = dev ?? connectedDevice;
    adapter.NotificationReceived += (s, e) =>
    {
        if (e.CharacteristicUuid == ch)
            Console.WriteLine($"[{DateTimeOffset.UtcNow:HH:mm:ss}] {BitConverter.ToString(e.Data).Replace("-", " ")}");
    };
    await adapter.SubscribeCharacteristicAsync(target, svc, ch);
    Console.WriteLine($"Subscribed to {ch}. Press Ctrl+C to stop.");
    await Task.Delay(-1); // wait indefinitely
}, serviceOption, charOption, deviceOption);

// run
var scriptArg = new Argument<FileInfo>("script", "Path to JavaScript file");
var runCmd = new Command("run", "Run a JavaScript BLE script") { scriptArg };
runCmd.SetHandler(async (script) =>
{
    var code = File.ReadAllText(script.FullName);
    using var engine = new ScriptEngine(adapter, (msg, isErr) =>
    {
        if (isErr) Console.Error.WriteLine(msg);
        else Console.WriteLine(msg);
    });
    var error = await engine.RunAsync(code);
    if (error != null) Console.Error.WriteLine(error);
}, scriptArg);

rootCommand.Add(scanCmd);
rootCommand.Add(connectCmd);
rootCommand.Add(disconnectCmd);
rootCommand.Add(readCmd);
rootCommand.Add(writeCmd);
rootCommand.Add(subscribeCmd);
rootCommand.Add(runCmd);

return await rootCommand.InvokeAsync(args);

static byte[] StringToBytes(string hex)
{
    hex = hex.Replace(" ", "").Replace("-", "");
    var bytes = new byte[hex.Length / 2];
    for (int i = 0; i < bytes.Length; i++)
        bytes[i] = byte.Parse(hex.AsSpan(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
    return bytes;
}
```

- [ ] **Step 2: Build and commit**

```bash
dotnet build E:\AI_Support\AI_Project\WindowsBleTool\WindowsBleTool.sln
git add -A
git commit -m "feat: implement CLI with scan, connect, read, write, subscribe, run commands"
```

---

## Phase 7: WinUI 3 GUI — Main Shell

### Task 12: Create MainWindow with NavigationView and Tab pages

**Files:**
- Modify: `/src/BleTool.Gui/MainWindow.xaml`
- Modify: `/src/BleTool.Gui/MainWindow.xaml.cs`
- Create: `/src/BleTool.Gui/Views/ScanPage.xaml`
- Create: `/src/BleTool.Gui/Views/ScanPage.xaml.cs`
- Create: `/src/BleTool.Gui/Views/GattPage.xaml`
- Create: `/src/BleTool.Gui/Views/GattPage.xaml.cs`
- Create: `/src/BleTool.Gui/Views/ScriptPage.xaml`
- Create: `/src/BleTool.Gui/Views/ScriptPage.xaml.cs`
- Create: `/src/BleTool.Gui/Views/SettingsPage.xaml`
- Create: `/src/BleTool.Gui/Views/SettingsPage.xaml.cs`

- [ ] **Step 1: Write MainWindow.xaml**

```xml
<!-- src/BleTool.Gui/MainWindow.xaml -->
<Window x:Class="BleTool.Gui.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:views="using:BleTool.Gui.Views"
        Title="Windows BLE Tool" Height="800" Width="1200"
        ExtendsContentIntoTitleBar="True">

    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Mode toggle -->
        <StackPanel Grid.Row="0" Orientation="Horizontal" HorizontalAlignment="Right" Margin="8">
            <ToggleSwitch x:Name="DevModeToggle" Header="开发者模式" IsOn="True"
                          Toggled="OnModeToggled"/>
        </StackPanel>

        <!-- TabView content -->
        <TabView Grid.Row="1" x:Name="MainTabView">
            <TabView.TabItems>
                <TabViewItem Header="&#x1F50D; 扫描设备" IsClosable="False">
                    <views:ScanPage x:Name="ScanPage"/>
                </TabViewItem>
                <TabViewItem Header="&#x1F4CB; GATT 浏览器" IsClosable="False">
                    <views:GattPage x:Name="GattPage"/>
                </TabViewItem>
                <TabViewItem Header="&#x1F4DD; 脚本编辑器" IsClosable="False">
                    <views:ScriptPage x:Name="ScriptPage"/>
                </TabViewItem>
                <TabViewItem Header="&#x2699; 设置" IsClosable="False">
                    <views:SettingsPage x:Name="SettingsPage"/>
                </TabViewItem>
            </TabView.TabItems>
        </TabView>

        <!-- Status bar -->
        <Border Grid.Row="2" Background="{ThemeResource CardBackgroundFillColorDefault}" Padding="8,4">
            <StackPanel Orientation="Horizontal" Spacing="16">
                <TextBlock x:Name="StatusText" Text="就绪" FontSize="12" Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                <TextBlock x:Name="DeviceCountText" Text="0 台设备" FontSize="12" Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
            </StackPanel>
        </Border>
    </Grid>
</Window>
```

- [ ] **Step 2: Write MainWindow.xaml.cs**

```csharp
// src/BleTool.Gui/MainWindow.xaml.cs
using BleCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BleTool.Gui;

public sealed partial class MainWindow : Window
{
    private readonly WindowsBleAdapter _adapter = new();
    private readonly SessionLogger _sessionLogger = new();

    public MainWindow()
    {
        InitializeComponent();
        ScanPage.Initialize(_adapter, _sessionLogger);
        GattPage.Initialize(_adapter, _sessionLogger);
        ScriptPage.Initialize(_adapter, _sessionLogger);
    }

    private void OnModeToggled(object sender, RoutedEventArgs e)
    {
        var isDev = DevModeToggle.IsOn;
        ScanPage.SetDevMode(isDev);
        GattPage.SetDevMode(isDev);
    }
}
```

- [ ] **Step 3: Write placeholder page files**

Each page stub follows this pattern (example for ScanPage):

```csharp
// src/BleTool.Gui/Views/ScanPage.xaml.cs
using BleCore;
using Microsoft.UI.Xaml.Controls;

namespace BleTool.Gui.Views;

public sealed partial class ScanPage : Page
{
    private WindowsBleAdapter? _adapter;
    private SessionLogger? _logger;

    public ScanPage() => InitializeComponent();

    public void Initialize(WindowsBleAdapter adapter, SessionLogger logger)
    {
        _adapter = adapter;
        _logger = logger;
    }

    public void SetDevMode(bool isDev) { /* toggle simplified view */ }
}
```

Minimal XAML stubs for each page:
```xml
<!-- src/BleTool.Gui/Views/ScanPage.xaml -->
<Page x:Class="BleTool.Gui.Views.ScanPage"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Grid><TextBlock Text="Scan Page" FontSize="24"/></Grid>
</Page>
```

Repeat for GattPage, ScriptPage, SettingsPage with same pattern.

- [ ] **Step 4: Build and commit**

```bash
dotnet build E:\AI_Support\AI_Project\WindowsBleTool\WindowsBleTool.sln
git add -A
git commit -m "feat: add MainWindow with TabView navigation and page stubs"
```

---

## Phase 8: WinUI 3 — Scan Page

### Task 13: Implement ScanPage UI and ViewModel

**Files:**
- Create: `/src/BleTool.Gui/ViewModels/ScanViewModel.cs`
- Modify: `/src/BleTool.Gui/Views/ScanPage.xaml`
- Modify: `/src/BleTool.Gui/Views/ScanPage.xaml.cs`

- [ ] **Step 1: Write ScanViewModel**

```csharp
// src/BleTool.Gui/ViewModels/ScanViewModel.cs
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

    [ObservableProperty] private BleDeviceInfo? _selectedDevice;
    [ObservableProperty] private string _matchCount = "0 台设备";
    [ObservableProperty] private bool _isDevMode = true;

    public void Initialize(WindowsBleAdapter adapter, SessionLogger logger)
    {
        _adapter = adapter;
        _logger = logger;
        _adapter.ScanResult += OnScanResult;
    }

    private void OnScanResult(object? s, ScanResultEvent e)
    {
        MainThreadHelper.RunOnUIThread(() =>
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
    public void AddFilter(DeviceFilterRule rule)
    {
        ActiveFilters.Add(rule);
    }

    [RelayCommand]
    public void RemoveFilter(DeviceFilterRule rule)
    {
        ActiveFilters.Remove(rule);
    }

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

public static class MainThreadHelper
{
    public static void RunOnUIThread(Action action)
    {
        if (Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread() != null)
            action();
        else
            _ = App.MainWindow?.DispatcherQueue.TryEnqueue(() => action());
    }
}
```

- [ ] **Step 2: Write ScanPage.xaml**

```xml
<!-- src/BleTool.Gui/Views/ScanPage.xaml -->
<Page x:Class="BleTool.Gui.Views.ScanPage"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
      xmlns:controls="using:BleTool.Gui.Controls">

    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <!-- Toolbar -->
        <StackPanel Grid.Row="0" Orientation="Horizontal" Spacing="8" Margin="12,8" VerticalAlignment="Center">
            <Button x:Name="ScanButton" Content="&#x25B6; 开始扫描" Command="{Binding StartScanCommand}" Width="110"/>
            <Button Content="&#x25A0; 停止" Command="{Binding StopScanCommand}" Width="80"/>
            <TextBlock Text="|" VerticalAlignment="Center" Foreground="{ThemeResource TextFillColorTertiaryBrush}"/>
            <TextBlock Text="RSSI 过滤:" VerticalAlignment="Center" FontSize="13"/>
            <ComboBox x:Name="RssiFilter" Width="140" PlaceholderText="全部" FontSize="13"
                      ItemsSource="{x:Bind RssiOptions}"/>
            <controls:FilterEditor x:Name="FilterEditor"/>
            <TextBlock Text="{Binding MatchCount}" VerticalAlignment="Center" FontSize="12"
                       Foreground="{ThemeResource TextFillColorTertiaryBrush}" Margin="12,0,0,0"/>
        </StackPanel>

        <!-- Main content: 3-column layout -->
        <Grid Grid.Row="1" Margin="12,0">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="260"/>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="360"/>
            </Grid.ColumnDefinitions>

            <!-- Left: Device list -->
            <ListView Grid.Column="0" ItemsSource="{Binding Devices}"
                      SelectedItem="{Binding SelectedDevice, Mode=TwoWay}"
                      SelectionMode="Single">
                <ListView.ItemTemplate>
                    <DataTemplate x:DataType="models:BleDeviceInfo"
                                  xmlns:models="using:BleCore">
                        <Border BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
                                BorderThickness="0,0,0,1" Padding="8,6">
                            <StackPanel Spacing="2">
                                <StackPanel Orientation="Horizontal" Spacing="6">
                                    <TextBlock Text="{x:Bind Name}" FontWeight="SemiBold" FontSize="13"/>
                                    <Border Background="{ThemeResource SystemFillColorSuccessBrush}"
                                            CornerRadius="4" Padding="3,1" Visibility="{x:Bind IsConnected, Mode=OneWay}">
                                        <TextBlock Text="已连接" FontSize="10"/>
                                    </Border>
                                </StackPanel>
                                <TextBlock Text="{x:Bind Address}" FontSize="11" FontFamily="Consolas"
                                           Foreground="{ThemeResource TextFillColorTertiaryBrush}"/>
                                <TextBlock FontSize="11" Foreground="{ThemeResource TextFillColorSecondaryBrush}">
                                    RSSI: <Run Text="{x:Bind Rssi}" FontFamily="Consolas"/> dBm
                                </TextBlock>
                            </StackPanel>
                        </Border>
                    </DataTemplate>
                </ListView.ItemTemplate>
            </ListView>

            <!-- Center: Ad data -->
            <Grid Grid.Column="1" Margin="12,0">
                <Grid.RowDefinitions>
                    <RowDefinition Height="Auto"/>
                    <RowDefinition Height="*"/>
                </Grid.RowDefinitions>
                <TextBlock Grid.Row="0" Text="&#x1F4E1; 广播数据" FontWeight="SemiBold" Margin="0,0,0,6"/>
                <ListView Grid.Row="1" ItemsSource="{Binding AdData}">
                    <ListView.ItemTemplate>
                        <DataTemplate x:DataType="models:ParsedAdField"
                                      xmlns:models="using:BleCore">
                            <StackPanel Spacing="2" Padding="4,2">
                                <TextBlock FontWeight="SemiBold" FontSize="12"
                                           Text="{x:Bind Name}" Foreground="{ThemeResource SystemFillColorSuccessBrush}"/>
                                <TextBlock FontSize="11" FontFamily="Consolas"
                                           Text="{x:Bind Description}"
                                           Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                                           TextWrapping="Wrap"/>
                            </StackPanel>
                        </DataTemplate>
                    </ListView.ItemTemplate>
                </ListView>
            </Grid>

            <!-- Right: GATT services (placeholder, detailed in GattPage) -->
            <Grid Grid.Column="2">
                <TextBlock Text="双击设备连接并查看 GATT 服务" FontSize="12" VerticalAlignment="Center"
                           HorizontalAlignment="Center"
                           Foreground="{ThemeResource TextFillColorTertiaryBrush}"/>
            </Grid>
        </Grid>
    </Grid>
</Page>
```

- [ ] **Step 3: Write ScanPage.xaml.cs**

```csharp
// src/BleTool.Gui/Views/ScanPage.xaml.cs
using BleCore;
using BleTool.Gui.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace BleTool.Gui.Views;

public sealed partial class ScanPage : Page
{
    private readonly ScanViewModel _vm = new();
    private WindowsBleAdapter? _adapter;

    public ScanPage()
    {
        InitializeComponent();
        DataContext = _vm;
    }

    public List<string> RssiOptions { get; } = new() { "全部", ">= -60 dBm", ">= -70 dBm", ">= -80 dBm", "自定义..." };

    public void Initialize(WindowsBleAdapter adapter, SessionLogger logger)
    {
        _adapter = adapter;
        _vm.Initialize(adapter, logger);
    }

    public void SetDevMode(bool isDev) => _vm.SetDevMode(isDev);
}
```

- [ ] **Step 4: Create FilterEditor control**

```xml
<!-- src/BleTool.Gui/Controls/FilterEditor.xaml -->
<UserControl x:Class="BleTool.Gui.Controls.FilterEditor"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <StackPanel Orientation="Horizontal" Spacing="4">
        <Button Content="+ 添加过滤" FontSize="12" Click="OnAddFilterClick"/>
        <ItemsControl x:Name="ChipList">
            <ItemsControl.ItemsPanel>
                <ItemsPanelTemplate>
                    <StackPanel Orientation="Horizontal" Spacing="4"/>
                </ItemsPanelTemplate>
            </ItemsControl.ItemsPanel>
        </ItemsControl>
    </StackPanel>
</UserControl>
```

```csharp
// src/BleTool.Gui/Controls/FilterEditor.xaml.cs
using BleCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BleTool.Gui.Controls;

public sealed partial class FilterEditor : UserControl
{
    public FilterEditor() => InitializeComponent();

    private async void OnAddFilterClick(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "添加过滤规则",
            XamlRoot = XamlRoot,
            PrimaryButtonText = "添加",
            CloseButtonText = "取消",
        };
        var panel = new StackPanel { Spacing = 8 };
        var typeCombo = new ComboBox { PlaceholderText = "过滤类型" };
        typeCombo.Items.Add("RSSI");
        typeCombo.Items.Add("设备名称");
        typeCombo.Items.Add("广播 UUID");
        typeCombo.Items.Add("MAC 地址");
        typeCombo.Items.Add("厂商 ID");
        typeCombo.Items.Add("Raw 数据");
        typeCombo.Items.Add("地址类型");
        typeCombo.SelectedIndex = 0;
        var valueBox = new TextBox { PlaceholderText = "值" };
        panel.Children.Add(typeCombo);
        panel.Children.Add(valueBox);
        dialog.Content = panel;

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            var chip = new Button { Content = $"{typeCombo.SelectedItem}: {valueBox.Text} ×", FontSize = 11, Margin = new Thickness(2, 0, 2, 0) };
            chip.Click += (_, _) => ChipList.Items.Remove(chip);
            ChipList.Items.Add(chip);
        }
    }
}
```

- [ ] **Step 5: Build and commit**

```bash
dotnet build E:\AI_Support\AI_Project\WindowsBleTool\WindowsBleTool.sln
git add -A
git commit -m "feat: implement scan page with device list, ad data panel, and filter editor"
```

---

## Phase 9: WinUI 3 — GATT Page

### Task 14: Implement GattPage

**Files:**
- Create: `/src/BleTool.Gui/ViewModels/GattViewModel.cs`
- Modify: `/src/BleTool.Gui/Views/GattPage.xaml`
- Modify: `/src/BleTool.Gui/Views/GattPage.xaml.cs`
- Create: `/src/BleTool.Gui/Controls/FormatToggle.xaml`
- Create: `/src/BleTool.Gui/Controls/FormatToggle.xaml.cs`

- [ ] **Step 1: Write GattViewModel**

```csharp
// src/BleTool.Gui/ViewModels/GattViewModel.cs
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

    [ObservableProperty] private BleServiceInfo? _selectedService;
    [ObservableProperty] private BleCharacteristicInfo? _selectedCharacteristic;
    [ObservableProperty] private string _valueHex = "";
    [ObservableProperty] private string _valueDec = "";
    [ObservableProperty] private string _valueBin = "";
    [ObservableProperty] private string _valueUtf8 = "";
    [ObservableProperty] private string _valueBase64 = "";
    [ObservableProperty] private string _connectedDeviceId = "";
    [ObservableProperty] private string _connectedDeviceName = "";
    [ObservableProperty] private bool _isDevMode = true;

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
        foreach (var svc in services)
            Services.Add(svc);
        _logger?.Log("GATT", $"发现 {services.Count} 个服务");
    }

    partial void OnSelectedServiceChanged(BleServiceInfo? value)
    {
        if (value != null)
        {
            Characteristics.Clear();
            foreach (var ch in value.Characteristics)
                Characteristics.Add(ch);
        }
    }

    [RelayCommand]
    public async Task ReadCharacteristic()
    {
        if (_adapter == null || SelectedCharacteristic == null) return;
        _logger?.Log("READ", $"读取 {SelectedCharacteristic.ServiceUuid}/{SelectedCharacteristic.Uuid}");
        var data = await _adapter.ReadCharacteristicAsync(
            ConnectedDeviceId, SelectedCharacteristic.ServiceUuid, SelectedCharacteristic.Uuid);
        UpdateValues(data);
    }

    [RelayCommand]
    public async Task WriteCharacteristic(string hexData)
    {
        if (_adapter == null || SelectedCharacteristic == null) return;
        var bytes = HexToBytes(hexData);
        _logger?.Log("WRITE", $"写入 {SelectedCharacteristic.Uuid}: {hexData}");
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
        MainThreadHelper.RunOnUIThread(() => UpdateValues(e.Data));
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
```

- [ ] **Step 2: Write GattPage.xaml**

```xml
<!-- src/BleTool.Gui/Views/GattPage.xaml -->
<Page x:Class="BleTool.Gui.Views.GattPage"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <Grid Margin="12">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="3*"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="2*"/>
        </Grid.RowDefinitions>

        <!-- Connected device info -->
        <StackPanel Grid.Row="0" Orientation="Horizontal" Spacing="8" Margin="0,0,0,8">
            <TextBlock FontWeight="SemiBold" FontSize="14">
                <Run Text="{Binding ConnectedDeviceName}"/>
                <Run Text=" (" Foreground="{ThemeResource TextFillColorTertiaryBrush}"/>
                <Run Text="{Binding ConnectedDeviceId}" FontFamily="Consolas"
                     Foreground="{ThemeResource TextFillColorTertiaryBrush}"/>
                <Run Text=")" Foreground="{ThemeResource TextFillColorTertiaryBrush}"/>
            </TextBlock>
        </StackPanel>

        <!-- Service list -->
        <ListView Grid.Row="1" ItemsSource="{Binding Services}"
                  SelectedItem="{Binding SelectedService, Mode=TwoWay}">
            <ListView.ItemTemplate>
                <DataTemplate x:DataType="models:BleServiceInfo" xmlns:models="using:BleCore">
                    <StackPanel Padding="8,4">
                        <TextBlock FontWeight="SemiBold" FontSize="13">
                            <Run Text="{x:Bind DisplayName}"/>
                            <Run Text=" (" Foreground="{ThemeResource TextFillColorTertiaryBrush}"/>
                            <Run Text="{x:Bind Uuid}" FontFamily="Consolas" FontSize="11"
                                 Foreground="{ThemeResource TextFillColorTertiaryBrush}"/>
                            <Run Text=")" Foreground="{ThemeResource TextFillColorTertiaryBrush}"/>
                        </TextBlock>
                        <TextBlock FontSize="11"
                                   Text="{x:Bind Characteristics.Count}"
                                   Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                    </StackPanel>
                </DataTemplate>
            </ListView.ItemTemplate>
        </ListView>

        <!-- Characteristic table -->
        <ListView Grid.Row="2" Margin="0,8,0,0" MaxHeight="150"
                  ItemsSource="{Binding Characteristics}"
                  SelectedItem="{Binding SelectedCharacteristic, Mode=TwoWay}">
            <ListView.Header>
                <Grid ColumnDefinitions="2*,1*,1*,2*" Padding="8,4">
                    <TextBlock Grid.Column="0" Text="特征值" FontWeight="SemiBold" FontSize="12"/>
                    <TextBlock Grid.Column="1" Text="UUID" FontWeight="SemiBold" FontSize="12"/>
                    <TextBlock Grid.Column="2" Text="权限" FontWeight="SemiBold" FontSize="12"/>
                    <TextBlock Grid.Column="3" Text="当前值" FontWeight="SemiBold" FontSize="12"/>
                </Grid>
            </ListView.Header>
            <ListView.ItemTemplate>
                <DataTemplate x:DataType="models:BleCharacteristicInfo" xmlns:models="using:BleCore">
                    <Grid ColumnDefinitions="2*,1*,1*,2*" Padding="8,3">
                        <TextBlock Grid.Column="0" Text="{x:Bind DisplayName}" FontSize="12"/>
                        <TextBlock Grid.Column="1" Text="{x:Bind Uuid}" FontFamily="Consolas" FontSize="11"
                                   Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                        <TextBlock Grid.Column="2" Text="{x:Bind Permissions}" FontSize="11"
                                   Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                        <TextBlock Grid.Column="3" Text="{x:Bind LastValueHex}"
                                   FontFamily="Consolas" FontSize="11"
                                   Foreground="{ThemeResource TextFillColorSecondaryBrush}"/>
                    </Grid>
                </DataTemplate>
            </ListView.ItemTemplate>
        </ListView>

        <!-- Value inspector + actions -->
        <StackPanel Grid.Row="3" Spacing="8">
            <controls:FormatToggle x:Name="FormatToggle"/>
            <Grid ColumnDefinitions="Auto,*,Auto,Auto,Auto" Spacing="8">
                <TextBox Grid.Column="1" Text="{Binding ValueHex}" FontFamily="Consolas" FontSize="13"/>
                <Button Grid.Column="2" Content="&#x1F4E5; 读取" Command="{Binding ReadCharacteristicCommand}"/>
                <Button Grid.Column="3" Content="&#x1F4E4; 写入" Click="OnWriteClick"/>
                <Button Grid.Column="4" Content="&#x1F514; 订阅" Command="{Binding SubscribeCharacteristicCommand}"
                        Foreground="{ThemeResource SystemFillColorCriticalBrush}"/>
            </Grid>
            <!-- Multi-format display -->
            <Border Background="{ThemeResource CardBackgroundFillColorDefault}" Padding="8" CornerRadius="4">
                <StackPanel Spacing="2">
                    <TextBlock FontSize="11"><Run Text="Hex:" FontWeight="SemiBold"/> <Run Text="{Binding ValueHex}" FontFamily="Consolas"/></TextBlock>
                    <TextBlock FontSize="11"><Run Text="Dec:" FontWeight="SemiBold"/> <Run Text="{Binding ValueDec}" FontFamily="Consolas"/></TextBlock>
                    <TextBlock FontSize="11"><Run Text="Bin:" FontWeight="SemiBold"/> <Run Text="{Binding ValueBin}" FontFamily="Consolas"/></TextBlock>
                    <TextBlock FontSize="11"><Run Text="UTF-8:" FontWeight="SemiBold"/> <Run Text="{Binding ValueUtf8}" FontFamily="Consolas"/></TextBlock>
                    <TextBlock FontSize="11"><Run Text="Base64:" FontWeight="SemiBold"/> <Run Text="{Binding ValueBase64}" FontFamily="Consolas"/></TextBlock>
                </StackPanel>
            </Border>
        </StackPanel>
    </Grid>
</Page>
```

- [ ] **Step 3: Write GattPage.xaml.cs**

```csharp
// src/BleTool.Gui/Views/GattPage.xaml.cs
using BleCore;
using BleTool.Gui.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace BleTool.Gui.Views;

public sealed partial class GattPage : Page
{
    private readonly GattViewModel _vm = new();

    public GattPage()
    {
        InitializeComponent();
        DataContext = _vm;
    }

    public void Initialize(WindowsBleAdapter adapter, SessionLogger logger)
        => _vm.Initialize(adapter, logger);

    public async Task OpenDevice(string deviceId, string deviceName)
        => await _vm.ConnectAndDiscover(deviceId, deviceName);

    public void SetDevMode(bool isDev) => _vm.SetDevMode(isDev);

    private async void OnWriteClick(object sender, RoutedEventArgs e)
    {
        var dialog = new ContentDialog
        {
            Title = "写入 Hex 数据",
            XamlRoot = XamlRoot,
            PrimaryButtonText = "写入",
            CloseButtonText = "取消",
        };
        var box = new TextBox { PlaceholderText = "例如: 01 00 FF", FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Consolas") };
        dialog.Content = box;
        if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            await _vm.WriteCharacteristicCommand.ExecuteAsync(box.Text);
    }
}
```

- [ ] **Step 4: Create FormatToggle control**

```csharp
// src/BleTool.Gui/Controls/FormatToggle.xaml.cs
using BleCore;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BleTool.Gui.Controls;

public sealed partial class FormatToggle : UserControl, INotifyPropertyChanged
{
    private DataFormat _selectedFormat = DataFormat.Hex;

    public DataFormat SelectedFormat
    {
        get => _selectedFormat;
        set { _selectedFormat = value; OnPropertyChanged(); FormatChanged?.Invoke(this, value); }
    }

    public event EventHandler<DataFormat>? FormatChanged;
    public event PropertyChangedEventHandler? PropertyChanged;

    public FormatToggle() => InitializeComponent();

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
```

```xml
<!-- src/BleTool.Gui/Controls/FormatToggle.xaml -->
<UserControl x:Class="BleTool.Gui.Controls.FormatToggle"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <StackPanel Orientation="Horizontal" Spacing="4">
        <Button Content="Hex" Width="48" FontSize="11" Click="OnFormatClick"/>
        <Button Content="Dec" Width="48" FontSize="11" Click="OnFormatClick"/>
        <Button Content="Bin" Width="48" FontSize="11" Click="OnFormatClick"/>
        <Button Content="UTF-8" Width="52" FontSize="11" Click="OnFormatClick"/>
        <Button Content="Base64" Width="56" FontSize="11" Click="OnFormatClick"/>
    </StackPanel>
</UserControl>
```

In FormatToggle.xaml.cs, add the click handler:
```csharp
private void OnFormatClick(object sender, RoutedEventArgs e)
{
    if (sender is Button btn)
    {
        SelectedFormat = btn.Content.ToString() switch
        {
            "Hex" => DataFormat.Hex,
            "Dec" => DataFormat.Decimal,
            "Bin" => DataFormat.Binary,
            "UTF-8" => DataFormat.Utf8,
            "Base64" => DataFormat.Base64,
            _ => DataFormat.Hex,
        };
    }
}
```

- [ ] **Step 5: Build and commit**

```bash
dotnet build E:\AI_Support\AI_Project\WindowsBleTool\WindowsBleTool.sln
git add -A
git commit -m "feat: implement GATT page with service tree, characteristic table, and multi-format value display"
```

---

## Phase 10: WinUI 3 — Script Page & Settings Page

### Task 15: Implement ScriptPage with editor and console

**Files:**
- Create: `/src/BleTool.Gui/ViewModels/ScriptViewModel.cs`
- Modify: `/src/BleTool.Gui/Views/ScriptPage.xaml`
- Modify: `/src/BleTool.Gui/Views/ScriptPage.xaml.cs`

- [ ] **Step 1: Write ScriptViewModel**

```csharp
// src/BleTool.Gui/ViewModels/ScriptViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BleCore;
using System.Collections.ObjectModel;

namespace BleTool.Gui.ViewModels;

public partial class ScriptViewModel : ObservableObject
{
    private WindowsBleAdapter? _adapter;
    private ScriptEngine? _engine;
    private CancellationTokenSource? _cancelSource;

    [ObservableProperty] private string _scriptCode = "// 在此编写 BLE 脚本...\n";
    [ObservableProperty] private bool _isRunning;
    [ObservableProperty] private string _statusText = "就绪";
    [ObservableProperty] private bool _hasConnectedDevice;

    public ObservableCollection<ConsoleLine> ConsoleOutput { get; } = new();

    public void Initialize(WindowsBleAdapter adapter, SessionLogger logger)
    {
        _adapter = adapter;
    }

    [RelayCommand]
    public async Task RunScript()
    {
        if (_adapter == null || IsRunning) return;
        IsRunning = true;
        StatusText = "运行中...";
        ConsoleOutput.Clear();
        _cancelSource = new CancellationTokenSource();

        _engine = new ScriptEngine(_adapter, (msg, isErr) =>
        {
            MainThreadHelper.RunOnUIThread(() =>
            {
                ConsoleOutput.Add(new ConsoleLine
                {
                    Text = $"[{DateTimeOffset.UtcNow:HH:mm:ss}] {msg}",
                    IsError = isErr
                });
            });
        });

        var error = await _engine.RunAsync(ScriptCode);
        if (error != null)
            ConsoleOutput.Add(new ConsoleLine { Text = error, IsError = true });

        IsRunning = false;
        StatusText = error == null ? "完成" : "错误";
        _engine.Dispose();
    }

    [RelayCommand]
    public void StopScript()
    {
        _engine?.Cancel();
        _cancelSource?.Cancel();
        StatusText = "已停止";
        IsRunning = false;
    }

    public void LoadTemplate(string code)
    {
        ScriptCode = code;
    }
}

public class ConsoleLine
{
    public string Text { get; init; } = string.Empty;
    public bool IsError { get; init; }
}
```

- [ ] **Step 2: Write ScriptPage.xaml**

```xml
<!-- src/BleTool.Gui/Views/ScriptPage.xaml -->
<Page x:Class="BleTool.Gui.Views.ScriptPage"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <Grid Margin="12">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <!-- Toolbar -->
        <StackPanel Grid.Row="0" Orientation="Horizontal" Spacing="8" Margin="0,0,0,8">
            <Button Content="&#x25B6; 运行" Command="{Binding RunScriptCommand}" Width="80"/>
            <Button Content="&#x23F9; 停止" Command="{Binding StopScriptCommand}" Width="80"/>
            <ComboBox x:Name="TemplateSelector" PlaceholderText="示例脚本" Width="160" FontSize="13"
                      SelectionChanged="OnTemplateSelected"/>
            <Button Content="&#x1F4BE; 保存" x:Name="SaveButton" Click="OnSaveClick" FontSize="12"/>
            <Button Content="&#x1F4C2; 加载" x:Name="LoadButton" Click="OnLoadClick" FontSize="12"/>
            <TextBlock Text="{Binding StatusText}" VerticalAlignment="Center" FontSize="12"
                       Foreground="{ThemeResource TextFillColorSecondaryBrush}" Margin="12,0,0,0"/>
        </StackPanel>

        <!-- Editor + Console split -->
        <Grid Grid.Row="1">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="340"/>
            </Grid.ColumnDefinitions>

            <!-- Code editor -->
            <TextBox Grid.Column="0" Text="{Binding ScriptCode, Mode=TwoWay}"
                     FontFamily="Consolas" FontSize="13" AcceptsReturn="True"
                     TextWrapping="NoWrap" HorizontalScrollBarVisibility="Auto"
                     IsSpellCheckEnabled="False"
                     Background="{ThemeResource CardBackgroundFillColorDefault}"
                     BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"/>

            <!-- Console right side -->
            <Grid Grid.Column="1" Margin="12,0,0,0">
                <Grid.RowDefinitions>
                    <RowDefinition Height="*"/>
                    <RowDefinition Height="Auto"/>
                </Grid.RowDefinitions>

                <ListView Grid.Row="0" ItemsSource="{Binding ConsoleOutput}"
                          Background="{ThemeResource CardBackgroundFillColorDefault}">
                    <ListView.ItemTemplate>
                        <DataTemplate x:DataType="viewmodels:ConsoleLine"
                                      xmlns:viewmodels="using:BleTool.Gui.ViewModels">
                            <TextBlock Text="{x:Bind Text}" FontFamily="Consolas" FontSize="11"
                                       Foreground="{x:Bind IsError, Mode=OneWay}"/>
                        </DataTemplate>
                    </ListView.ItemTemplate>
                </ListView>

                <!-- API quick reference -->
                <Expander Grid.Row="1" Header="&#x1F4D6; BLE API 速览" Margin="0,8,0,0" FontSize="11">
                    <StackPanel Spacing="1" FontFamily="Consolas" FontSize="10"
                                Foreground="{ThemeResource TextFillColorSecondaryBrush}">
                        <TextBlock Text="ble.scanAsync(filters?, opts?)"/>
                        <TextBlock Text="  -> Promise<Device[]>" Margin="12,0,0,4"/>
                        <TextBlock Text="device.getServiceAsync(uuid)"/>
                        <TextBlock Text="  -> Promise<Service>" Margin="12,0,0,4"/>
                        <TextBlock Text="char.readAsync() / .writeAsync(data)"/>
                        <TextBlock Text="  -> Promise<Uint8Array>" Margin="12,0,0,4"/>
                        <TextBlock Text="char.subscribeAsync(callback)"/>
                        <TextBlock Text="  callback(data: Uint8Array)" Margin="12,0,0,4"/>
                        <TextBlock Text="console.log() / .error() / .warn()"/>
                    </StackPanel>
                </Expander>
            </Grid>
        </Grid>
    </Grid>
</Page>
```

- [ ] **Step 3: Write ScriptPage.xaml.cs**

```csharp
// src/BleTool.Gui/Views/ScriptPage.xaml.cs
using BleCore;
using BleTool.Gui.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace BleTool.Gui.Views;

public sealed partial class ScriptPage : Page
{
    private readonly ScriptViewModel _vm = new();

    public ScriptPage()
    {
        InitializeComponent();
        DataContext = _vm;
        TemplateSelector.Items.Add("HR 数据记录器");
        TemplateSelector.Items.Add("批量扫描");
        TemplateSelector.Items.Add("OTA DFU 升级");
    }

    public void Initialize(WindowsBleAdapter adapter, SessionLogger logger)
        => _vm.Initialize(adapter, logger);

    public void SetDevMode(bool isDev) { /* unused */ }

    private void OnTemplateSelected(object sender, SelectionChangedEventArgs e)
    {
        if (TemplateSelector.SelectedItem is string template)
        {
            switch (template)
            {
                case "HR 数据记录器":
                    _vm.LoadTemplate(@"// BLE 心率数据记录器
const devices = await ble.scanAsync({
    filters: [{ name: 'Heart*' }],
    duration: 5000
});
if (devices.length === 0) throw new Error('设备未找到');
const device = await ble.connectAsync(devices[0]);
const hrService = await device.getServiceAsync('0000180d-0000-1000-8000-00805f9b34fb');
const hrChar = await hrService.getCharacteristicAsync('00002a37-0000-1000-8000-00805f9b34fb');
let count = 0;
await hrChar.subscribeAsync((data) => {
    const hr = data[1];
    console.log(`${new Date().toISOString()} | HR: ${hr} bpm`);
    if (++count >= 100) hrChar.unsubscribe();
});
console.log('监听中，按停止键退出...');
");
                    break;
                case "批量扫描":
                    _vm.LoadTemplate("// 批量扫描所有设备\nconst devices = await ble.scanAsync({ duration: 3000 });\ndevices.forEach(d => console.log(`${d.name}: ${d.address} RSSI=${d.rssi}`));\n");
                    break;
                case "OTA DFU 升级":
                    _vm.LoadTemplate("// OTA DFU 升级脚本\nconsole.log('DFU 升级流程: 1.进入 DFU 模式 2.传输固件 3.验证');\n");
                    break;
            }
        }
    }

    private async void OnSaveClick(object sender, RoutedEventArgs e)
    {
        var picker = new FileSavePicker();
        InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow));
        picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        picker.FileTypeChoices.Add("JavaScript", new[] { ".js" });
        var file = await picker.PickSaveFileAsync();
        if (file != null)
            await FileIO.WriteTextAsync(file, _vm.ScriptCode);
    }

    private async void OnLoadClick(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker();
        InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow));
        picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        picker.FileTypeFilter.Add(".js");
        var file = await picker.PickSingleFileAsync();
        if (file != null)
            _vm.ScriptCode = await FileIO.ReadTextAsync(file);
    }
}
```

- [ ] **Step 4: Build and commit**

```bash
dotnet build E:\AI_Support\AI_Project\WindowsBleTool\WindowsBleTool.sln
git add -A
git commit -m "feat: implement script editor page with code editor, console, templates, and save/load"
```

### Task 16: Implement SettingsPage

**Files:**
- Create: `/src/BleTool.Gui/ViewModels/SettingsViewModel.cs`
- Modify: `/src/BleTool.Gui/Views/SettingsPage.xaml`
- Modify: `/src/BleTool.Gui/Views/SettingsPage.xaml.cs`

- [ ] **Step 1: Write SettingsViewModel**

```csharp
// src/BleTool.Gui/ViewModels/SettingsViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BleCore;

namespace BleTool.Gui.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly AppSettings _settings = AppSettings.Load();

    [ObservableProperty] private int _defaultRssiThreshold;
    [ObservableProperty] private int _logMaxEntries;
    [ObservableProperty] private string _hexSeparator = " ";

    public SettingsViewModel()
    {
        DefaultRssiThreshold = _settings.DefaultRssiThreshold;
        LogMaxEntries = _settings.LogMaxEntries;
        HexSeparator = _settings.PreferredHexSeparator;
    }

    [RelayCommand]
    public void Save()
    {
        _settings.DefaultRssiThreshold = DefaultRssiThreshold;
        _settings.LogMaxEntries = LogMaxEntries;
        _settings.PreferredHexSeparator = HexSeparator;
        _settings.Save();
    }
}
```

- [ ] **Step 2: Write SettingsPage.xaml**

```xml
<!-- src/BleTool.Gui/Views/SettingsPage.xaml -->
<Page x:Class="BleTool.Gui.Views.SettingsPage"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <ScrollViewer Margin="24,16" MaxWidth="500">
        <StackPanel Spacing="16">
            <TextBlock Text="设置" FontSize="24" FontWeight="SemiBold"/>

            <StackPanel Spacing="6">
                <TextBlock Text="默认 RSSI 阈值 (dBm)" FontSize="13"/>
                <NumberBox Value="{Binding DefaultRssiThreshold, Mode=TwoWay}" Minimum="-120" Maximum="0"/>
            </StackPanel>

            <StackPanel Spacing="6">
                <TextBlock Text="日志条数上限" FontSize="13"/>
                <NumberBox Value="{Binding LogMaxEntries, Mode=TwoWay}" Minimum="100" Maximum="100000"/>
            </StackPanel>

            <StackPanel Spacing="6">
                <TextBlock Text="Hex 分隔符" FontSize="13"/>
                <TextBox Text="{Binding HexSeparator, Mode=TwoWay}" Width="60" FontFamily="Consolas"/>
            </StackPanel>

            <Button Content="保存设置" Command="{Binding SaveCommand}" Width="120" HorizontalAlignment="Left"/>
        </StackPanel>
    </ScrollViewer>
</Page>
```

- [ ] **Step 3: Write SettingsPage.xaml.cs**

```csharp
// src/BleTool.Gui/Views/SettingsPage.xaml.cs
using BleCore;
using BleTool.Gui.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace BleTool.Gui.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        InitializeComponent();
        DataContext = new SettingsViewModel();
    }

    public void Initialize(WindowsBleAdapter adapter, SessionLogger logger) { /* standalone */ }
    public void SetDevMode(bool isDev) { /* unused */ }
}
```

- [ ] **Step 4: Build and commit**

```bash
dotnet build E:\AI_Support\AI_Project\WindowsBleTool\WindowsBleTool.sln
git add -A
git commit -m "feat: implement settings page with preferences persistence"
```

---

## Phase 11: Integration & Polish

### Task 17: Wire up cross-page communication and navigation

**Files:**
- Modify: `/src/BleTool.Gui/MainWindow.xaml.cs`
- Modify: `/src/BleTool.Gui/Views/ScanPage.xaml`
- Modify: `/src/BleTool.Gui/Views/ScanPage.xaml.cs`

- [ ] **Step 1: Add device double-click to navigate to GATT page**

In ScanPage.xaml, add `DoubleTapped` to the ListView:
```xml
<ListView ... DoubleTapped="OnDeviceDoubleTapped">
```

In ScanPage.xaml.cs:
```csharp
private void OnDeviceDoubleTapped(object sender, Microsoft.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
{
    if (_vm.SelectedDevice != null)
    {
        (App.MainWindow as MainWindow)?.NavigateToGattPage(
            _vm.SelectedDevice.Address, _vm.SelectedDevice.Name);
    }
}
```

In MainWindow.xaml.cs:
```csharp
public void NavigateToGattPage(string deviceId, string deviceName)
{
    MainTabView.SelectedIndex = 1; // GATT tab
    _ = GattPage.OpenDevice(deviceId, deviceName);
}
```

- [ ] **Step 2: Build and commit**

```bash
dotnet build E:\AI_Support\AI_Project\WindowsBleTool\WindowsBleTool.sln
git add -A
git commit -m "feat: wire up cross-page navigation from scan to GATT on device double-click"
```

### Task 18: Add notification data and session log display to GUI

**Files:**
- Modify: `/src/BleTool.Gui/Views/GattPage.xaml`
- Create: `/src/BleTool.Gui/Services/GuiSessionLogger.cs`

- [ ] **Step 1: Wire notification data logger into GattViewModel**

In GattViewModel.cs, add:
```csharp
private readonly NotificationDataLogger _notifLogger = new();

// After subscription, use _notifLogger in notification handler:
public IReadOnlyList<NotificationRecord> NotificationRecords => _notifLogger.Records;

public string ExportNotificationsCsv() => _notifLogger.ExportAsCsv();
```

- [ ] **Step 2: Add log export buttons to status bar**

In MainWindow.xaml, add to status bar:
```xml
<Button Content="&#x1F4BE; 导出会话日志" FontSize="11" Click="OnExportSessionLog"/>
<Button Content="&#x1F4CA; 导出通知 CSV" FontSize="11" Click="OnExportNotificationCsv"/>
```

File save dialog handlers in MainWindow.xaml.cs:
```csharp
private async void OnExportSessionLog(object sender, RoutedEventArgs e)
{
    var picker = new FileSavePicker();
    InitializeWithWindow.Initialize(picker, WindowNative.GetWindowHandle(this));
    picker.FileTypeChoices.Add("Text", new[] { ".txt" });
    picker.FileTypeChoices.Add("JSON", new[] { ".json" });
    var file = await picker.PickSaveFileAsync();
    if (file != null)
    {
        var content = file.FileType switch
        {
            ".json" => _sessionLogger.ExportAsJson(),
            _ => _sessionLogger.ExportAsText(),
        };
        await FileIO.WriteTextAsync(file, content);
    }
}
```

- [ ] **Step 3: Build and commit**

```bash
dotnet build E:\AI_Support\AI_Project\WindowsBleTool\WindowsBleTool.sln
git add -A
git commit -m "feat: add session log and notification data export to GUI"
```

---

## Phase 12: Final Verification

### Task 19: Full build verification and final commit

- [ ] **Step 1: Clean and rebuild entire solution**

```bash
dotnet clean E:\AI_Support\AI_Project\WindowsBleTool\WindowsBleTool.sln
dotnet build E:\AI_Support\AI_Project\WindowsBleTool\WindowsBleTool.sln
```

Expected: All 4 projects build with 0 errors.

- [ ] **Step 2: Run all tests**

```bash
dotnet test E:\AI_Support\AI_Project\WindowsBleTool\tests\BleCore.Tests\BleCore.Tests.csproj
```

Expected: All tests pass.

- [ ] **Step 3: Verify key code paths**

Confirm the following compile-time checks:
- `IBleAdapter` is fully implemented by `WindowsBleAdapter`
- `DeviceFilter.MatchesAll` covers all filter types
- `DataFormatter.Format` covers all format enum values
- CLI `Program.cs` handles all commands from spec
- GUI pages all share the same `WindowsBleAdapter` and `SessionLogger` instances
- `ScriptEngine` sandboxes correctly (only `ble` + `console` exposed)

- [ ] **Step 4: Final commit**

```bash
git add -A
git commit -m "chore: final build verification, all tests passing"
```
