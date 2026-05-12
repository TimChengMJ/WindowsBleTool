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
