using System.CommandLine;
using System.Text.Json;
using BleCore;
using ScriptEngine = BleCore.ScriptEngine;

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
        Console.WriteLine(JsonSerializer.Serialize(devices, new JsonSerializerOptions { WriteIndented = true }));
    else
    {
        foreach (var d in devices)
            Console.WriteLine($"  {d.Name,-24} {d.Address,-17} {d.Rssi,4}dBm");
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
    if (all) { adapter.Dispose(); Console.WriteLine("Disconnected all devices."); }
    else { await adapter.DisconnectAsync(address); Console.WriteLine($"Disconnected from {address}"); }
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
    var bytes = HexToBytes(data);
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
    await Task.Delay(-1);
}, serviceOption, charOption, deviceOption);

// run
var scriptArg = new Argument<FileInfo>("script", "Path to JavaScript file");
var runCmd = new Command("run", "Run a JavaScript BLE script") { scriptArg };
runCmd.SetHandler(async (script) =>
{
    var code = await File.ReadAllTextAsync(script.FullName);
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

static byte[] HexToBytes(string hex)
{
    hex = hex.Replace(" ", "").Replace("-", "");
    var bytes = new byte[hex.Length / 2];
    for (int i = 0; i < bytes.Length; i++)
        bytes[i] = byte.Parse(hex.AsSpan(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
    return bytes;
}
