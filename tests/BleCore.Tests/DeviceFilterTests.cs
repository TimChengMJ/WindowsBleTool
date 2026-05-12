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
