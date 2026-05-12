// src/BleCore/BleException.cs
namespace BleCore;

public class BleException : Exception
{
    public BleErrorCode Code { get; }

    public BleException(BleErrorCode code, string message, Exception? inner = null)
        : base(message, inner) => Code = code;
}

public enum BleErrorCode
{
    DeviceNotFound,
    DeviceUnreachable,
    ConnectionFailed,
    NotConnected,
    ServiceNotFound,
    CharacteristicNotFound,
    GattReadFailed,
    GattWriteFailed,
    CharacteristicNotReadable,
    CharacteristicNotWritable,
    CharacteristicNotNotifiable,
    SubscribeFailed,
    ScanError,
    AdapterUnavailable,
}
