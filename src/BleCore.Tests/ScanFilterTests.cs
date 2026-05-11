using BleCore.Models;

namespace BleCore.Tests;

public class ScanFilterTests
{
    private static BleDeviceInfo MakeDevice(string name = "Test", short rssi = -50, string address = "A4:C1:38:00:00:01")
    {
        return new BleDeviceInfo
        {
            Name = name,
            Rssi = rssi,
            BluetoothAddress = 0xA4C138000001,
            AdvertisementData = new AdvertisementData { Records = Array.Empty<AdvertisementRecord>() }
        };
    }

    [Fact]
    public void RssiFilter_GreaterOrEqual_Passes()
    {
        var device = MakeDevice(rssi: -50);
        var filters = new[] { new ScanFilter { Type = FilterType.Rssi, RssiThreshold = -60, RssiGreaterOrEqual = true } };

        Assert.True(ScanFilterEvaluator.Matches(device, filters, FilterLogic.And));
    }

    [Fact]
    public void RssiFilter_BelowThreshold_Fails()
    {
        var device = MakeDevice(rssi: -80);
        var filters = new[] { new ScanFilter { Type = FilterType.Rssi, RssiThreshold = -60, RssiGreaterOrEqual = true } };

        Assert.False(ScanFilterEvaluator.Matches(device, filters, FilterLogic.And));
    }

    [Fact]
    public void NameFilter_Contains_Passes()
    {
        var device = MakeDevice(name: "Heart Rate Sensor");
        var filters = new[] { new ScanFilter { Type = FilterType.DeviceName, NamePattern = "Heart", NameMatchMode = NameMatchMode.Contains } };

        Assert.True(ScanFilterEvaluator.Matches(device, filters, FilterLogic.And));
    }

    [Fact]
    public void NameFilter_NoMatch_Fails()
    {
        var device = MakeDevice(name: "Heart Rate Sensor");
        var filters = new[] { new ScanFilter { Type = FilterType.DeviceName, NamePattern = "Battery", NameMatchMode = NameMatchMode.Contains } };

        Assert.False(ScanFilterEvaluator.Matches(device, filters, FilterLogic.And));
    }

    [Fact]
    public void OrLogic_OneFilterMatches_Passes()
    {
        var device = MakeDevice(name: "Heart Rate Sensor", rssi: -85);
        var filters = new[]
        {
            new ScanFilter { Type = FilterType.Rssi, RssiThreshold = -60, RssiGreaterOrEqual = true },
            new ScanFilter { Type = FilterType.DeviceName, NamePattern = "Heart", NameMatchMode = NameMatchMode.Contains }
        };

        Assert.True(ScanFilterEvaluator.Matches(device, filters, FilterLogic.Or));
    }

    [Fact]
    public void AndLogic_BothFiltersMustMatch()
    {
        var device = MakeDevice(name: "Heart Rate Sensor", rssi: -50);
        var filters = new[]
        {
            new ScanFilter { Type = FilterType.Rssi, RssiThreshold = -60, RssiGreaterOrEqual = true },
            new ScanFilter { Type = FilterType.DeviceName, NamePattern = "Heart", NameMatchMode = NameMatchMode.Contains }
        };

        Assert.True(ScanFilterEvaluator.Matches(device, filters, FilterLogic.And));
    }

    [Fact]
    public void EmptyFilters_AlwaysPasses()
    {
        var device = MakeDevice();
        Assert.True(ScanFilterEvaluator.Matches(device, Array.Empty<ScanFilter>(), FilterLogic.And));
    }
}
