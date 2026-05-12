namespace BleCore;

public class BleServiceInfo
{
    private Guid? _guid;

    public string Uuid { get; init; } = string.Empty;
    public string DisplayName => GattKnownUuids.GetName(Uuid);
    public Guid Guid => _guid ??= Guid.Parse(Uuid);
    public List<BleCharacteristicInfo> Characteristics { get; init; } = new();
}
