using BleCore.Models;

namespace BleCore.Tests;

public class AdvertisementParserTests
{
    [Fact]
    public void Parse_FlagsRecord_ReturnsCorrectAdType()
    {
        byte[] raw = [0x02, 0x01, 0x06];

        var result = AdvertisementParser.Parse(raw);

        Assert.Single(result.Records);
        Assert.Equal(0x01, result.Records[0].AdType);
        Assert.Equal("Flags", result.Records[0].AdTypeName);
        Assert.Equal([0x06], result.Records[0].Data);
    }

    [Fact]
    public void Parse_CompleteLocalName_ReturnsUtf8String()
    {
        byte[] raw = [0x12, 0x09, 0x48, 0x65, 0x61, 0x72, 0x74, 0x20, 0x52, 0x61, 0x74, 0x65, 0x20, 0x53, 0x65, 0x6E, 0x73, 0x6F, 0x72];

        var result = AdvertisementParser.Parse(raw);

        Assert.Single(result.Records);
        Assert.Equal(0x09, result.Records[0].AdType);
        Assert.Equal("Heart Rate Sensor", System.Text.Encoding.UTF8.GetString(result.Records[0].Data));
    }

    [Fact]
    public void Parse_MultipleRecords_ReturnsAllRecords()
    {
        byte[] raw = [0x02, 0x01, 0x06, 0x03, 0x03, 0x0D, 0x18];

        var result = AdvertisementParser.Parse(raw);

        Assert.Equal(2, result.Records.Count);
        Assert.Equal(0x01, result.Records[0].AdType);
        Assert.Equal(0x03, result.Records[1].AdType);
    }

    [Fact]
    public void Parse_EmptyData_ReturnsNoRecords()
    {
        byte[] raw = [];

        var result = AdvertisementParser.Parse(raw);

        Assert.Empty(result.Records);
    }
}
