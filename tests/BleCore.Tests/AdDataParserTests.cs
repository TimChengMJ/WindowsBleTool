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
        var data = new byte[] { 0x0D, 0x18, 0x0A, 0x18 };
        var structures = new Dictionary<ushort, byte[]> { { 0x03, data } };
        var result = AdDataParser.Parse(structures);
        var field = result.First(f => f.AdType == 0x03);
        Assert.IsTrue(field.Description.Contains("0x180D"));
        Assert.IsTrue(field.Description.Contains("0x180A"));
    }
}
