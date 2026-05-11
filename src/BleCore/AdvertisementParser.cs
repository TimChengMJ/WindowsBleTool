using BleCore.Models;

namespace BleCore;

public static class AdvertisementParser
{
    public static AdvertisementData Parse(byte[] rawData)
    {
        var records = new List<AdvertisementRecord>();
        int i = 0;

        while (i < rawData.Length - 1)
        {
            byte length = rawData[i];
            if (length == 0 || i + length >= rawData.Length)
                break;

            byte adType = rawData[i + 1];
            int dataLength = length - 1;
            byte[] data = new byte[dataLength];
            Array.Copy(rawData, i + 2, data, 0, dataLength);

            records.Add(new AdvertisementRecord { AdType = adType, Data = data });
            i += length + 1;
        }

        return new AdvertisementData { Records = records, Timestamp = DateTimeOffset.Now };
    }
}
