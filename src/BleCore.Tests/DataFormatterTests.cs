using BleTool.Shared.DataFormat;

namespace BleCore.Tests;

public class DataFormatterTests
{
    [Fact]
    public void Format_Hex_ReturnsSpaceSeparatedHex()
    {
        byte[] data = [0x06, 0x48, 0x00];
        var result = DataFormatter.Format(data, DataFormatType.Hex);
        Assert.Equal("06 48 00", result);
    }

    [Fact]
    public void Format_Decimal_ReturnsSpaceSeparatedDecimals()
    {
        byte[] data = [0x06, 0x48, 0x00];
        var result = DataFormatter.Format(data, DataFormatType.Decimal);
        Assert.Equal("6 72 0", result);
    }

    [Fact]
    public void Format_Binary_Returns8BitPaddedBinaries()
    {
        byte[] data = [0x06, 0x48, 0x00];
        var result = DataFormatter.Format(data, DataFormatType.Binary);
        Assert.Equal("00000110 01001000 00000000", result);
    }

    [Fact]
    public void Format_Base64_ReturnsValidBase64()
    {
        byte[] data = [0x06, 0x48, 0x00];
        var result = DataFormatter.Format(data, DataFormatType.Base64);
        Assert.Equal("BkgA", result);
    }

    [Fact]
    public void ParseHex_Roundtrips()
    {
        byte[] original = [0x06, 0x48, 0x00];
        var hex = DataFormatter.Format(original, DataFormatType.Hex);
        var parsed = DataFormatter.ParseInput(hex, DataFormatType.Hex);
        Assert.Equal(original, parsed);
    }
}
