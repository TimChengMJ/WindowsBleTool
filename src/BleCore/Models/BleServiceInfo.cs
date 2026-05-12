namespace BleCore;

public class BleServiceInfo
{
    public string Uuid { get; init; } = string.Empty;
    public string DisplayName => GattKnownUuids.GetName(Uuid);
    public Guid Guid => Guid.Parse(Uuid);
    public List<BleCharacteristicInfo> Characteristics { get; init; } = new();
}
