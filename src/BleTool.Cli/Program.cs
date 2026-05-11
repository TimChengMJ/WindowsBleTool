using System.CommandLine;
using BleCore;
using BleCore.Models;
using BleTool.Shared.DataFormat;
using BleTool.Shared.Logging;

var scanner = new BleScanner();
var sessionLogger = new SessionLogger();
BleDevice? connectedDevice = null;
var dataFormat = DataFormatType.Hex;

var root = new RootCommand("Windows BLE Tool — CLI");

// scan command
var rssiOption = new Option<short?>("--rssi", "RSSI threshold filter");
var timeoutOption = new Option<int>("--timeout", () => 5, "Scan timeout in seconds");
var formatOption = new Option<string>("--format", () => "text", "Output format: text or json");
var scanCommand = new Command("scan", "Scan for BLE devices") { rssiOption, timeoutOption, formatOption };
scanCommand.SetHandler(async (short? rssi, int timeout, string format) =>
{
    sessionLogger.Log(LogCategory.Scan, "开始扫描...");
    if (rssi.HasValue)
        scanner.Filters = new[] { new ScanFilter { Type = FilterType.Rssi, RssiThreshold = rssi.Value } };

    scanner.DeviceDiscovered += d =>
        sessionLogger.Log(LogCategory.Scan, $"发现: {d.Name} ({d.AddressString}) RSSI={d.Rssi}dBm");

    scanner.Start();
    await Task.Delay(timeout * 1000);
    scanner.Stop();

    var devices = scanner.GetFilteredDevices();
    if (format == "json")
    {
        var json = System.Text.Json.JsonSerializer.Serialize(
            devices.Select(d => new { name = d.Name, address = d.AddressString, rssi = d.Rssi }),
            new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(json);
    }
    else
    {
        Console.WriteLine($"{"Name",-30} {"Address",-20} {"RSSI",-8}");
        Console.WriteLine(new string('-', 60));
        foreach (var d in devices)
            Console.WriteLine($"{d.Name,-30} {d.AddressString,-20} {d.Rssi,-8}");
    }
    sessionLogger.Log(LogCategory.Scan, $"扫描完成: {devices.Count} 台设备");
}, rssiOption, timeoutOption, formatOption);

// connect command
var addressArg = new Argument<string>("address", "Device MAC address");
var connectCommand = new Command("connect", "Connect to a BLE device") { addressArg };
connectCommand.SetHandler(async (string address) =>
{
    var info = scanner.AllDevices.FirstOrDefault(d =>
        d.AddressString.Equals(address, StringComparison.OrdinalIgnoreCase));
    if (info == null) { Console.Error.WriteLine($"设备 {address} 未找到"); Environment.Exit(1); return; }
    connectedDevice = new BleDevice(info);
    await connectedDevice.ConnectAsync();
    Console.WriteLine($"已连接到 {info.Name}");
    sessionLogger.Log(LogCategory.Connect, $"已连接: {info.Name} ({address})");
}, addressArg);

// disconnect command
var allOption = new Option<bool>("--all", () => false, "Disconnect all devices");
var disconnectCommand = new Command("disconnect", "Disconnect from device") { allOption };
disconnectCommand.SetHandler((bool all) =>
{
    if (all)
    {
        connectedDevice?.Disconnect();
        connectedDevice = null;
        Console.WriteLine("已断开所有连接");
    }
    else
    {
        connectedDevice?.Disconnect();
        connectedDevice = null;
        Console.WriteLine("已断开连接");
    }
}, allOption);

// list-services command
var listServicesCommand = new Command("list-services", "List GATT services");
listServicesCommand.SetHandler(async () =>
{
    if (connectedDevice == null) { Console.Error.WriteLine("未连接设备"); return; }
    var services = await connectedDevice.DiscoverServicesAsync();
    foreach (var svc in services)
    {
        Console.WriteLine($"  Service: {svc.Name} ({svc.Uuid})");
        foreach (var ch in svc.Characteristics)
            Console.WriteLine($"    Char: {ch.Name} ({ch.Uuid}) [{ch.Properties}]");
    }
});

// read command
var serviceOption = new Option<string>("--service", "Service UUID") { IsRequired = true };
var charOption = new Option<string>("--char", "Characteristic UUID") { IsRequired = true };
var readCommand = new Command("read", "Read characteristic value") { serviceOption, charOption, formatOption };
readCommand.SetHandler(async (string svcUuid, string charUuid, string fmt) =>
{
    if (connectedDevice == null) { Console.Error.WriteLine("未连接设备"); return; }
    var data = await connectedDevice.ReadCharacteristicAsync(Guid.Parse(svcUuid), Guid.Parse(charUuid));
    var df = Enum.Parse<DataFormatType>(fmt, ignoreCase: true);
    Console.WriteLine(DataFormatter.Format(data, df));
}, serviceOption, charOption, formatOption);

// write command
var dataOption = new Option<string>("--data", "Hex data to write") { IsRequired = true };
var withResponseOption = new Option<bool>("--with-response", () => true, "Write with response");
var withoutResponseOption = new Option<bool>("--without-response", () => false, "Write without response");
var writeCommand = new Command("write", "Write characteristic value")
    { serviceOption, charOption, dataOption, withResponseOption, withoutResponseOption };
writeCommand.SetHandler(async (string svcUuid, string charUuid, string dataHex, bool withResp, bool withoutResp) =>
{
    if (connectedDevice == null) { Console.Error.WriteLine("未连接设备"); return; }
    var data = DataFormatter.ParseInput(dataHex, DataFormatType.Hex);
    await connectedDevice.WriteCharacteristicAsync(Guid.Parse(svcUuid), Guid.Parse(charUuid), data, !withoutResp);
    Console.WriteLine("写入成功");
}, serviceOption, charOption, dataOption, withResponseOption, withoutResponseOption);

// subscribe command
var subscribeCommand = new Command("subscribe", "Subscribe to characteristic notifications")
    { serviceOption, charOption };
subscribeCommand.SetHandler(async (string svcUuid, string charUuid) =>
{
    if (connectedDevice == null) { Console.Error.WriteLine("未连接设备"); return; }
    Console.WriteLine($"订阅 {charUuid} 通知中... (按 Ctrl+C 退出)");
    await connectedDevice.SubscribeAsync(Guid.Parse(svcUuid), Guid.Parse(charUuid), data =>
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {DataFormatter.Format(data, dataFormat)}");
    });
    await Task.Delay(Timeout.Infinite);
}, serviceOption, charOption);

// run command
var scriptArg = new Argument<FileInfo>("script-file", "JavaScript file to execute");
var runCommand = new Command("run", "Execute a BLE script") { scriptArg };
runCommand.SetHandler(async (FileInfo file) =>
{
    if (!file.Exists) { Console.Error.WriteLine($"File not found: {file}"); return; }
    var script = await File.ReadAllTextAsync(file.FullName);
    var engine = new global::BleTool.Shared.Scripting.ScriptEngine(scanner);
    engine.ConsoleOutput += Console.WriteLine;
    engine.ErrorOutput += Console.Error.WriteLine;
    await engine.ExecuteAsync(script);
    engine.Dispose();
}, scriptArg);

root.AddCommand(scanCommand);
root.AddCommand(connectCommand);
root.AddCommand(disconnectCommand);
root.AddCommand(listServicesCommand);
root.AddCommand(readCommand);
root.AddCommand(writeCommand);
root.AddCommand(subscribeCommand);
root.AddCommand(runCommand);

return await root.InvokeAsync(args);
