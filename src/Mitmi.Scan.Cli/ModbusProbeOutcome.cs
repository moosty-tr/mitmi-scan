namespace Mitmi.Scan.Cli;

public sealed record ModbusProbeOutcome(
    ScanResultStatus Status,
    ScanValue? Value,
    byte? ModbusExceptionCode,
    string? Message)
{
    public static ModbusProbeOutcome Readable(ScanValue value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return new ModbusProbeOutcome(ScanResultStatus.Readable, value, ModbusExceptionCode: null, Message: null);
    }

    public static ModbusProbeOutcome ModbusException(byte exceptionCode, string? message)
    {
        return new ModbusProbeOutcome(ScanResultStatus.ModbusException, Value: null, exceptionCode, NormalizeMessage(message));
    }

    public static ModbusProbeOutcome Timeout(string? message)
    {
        return new ModbusProbeOutcome(ScanResultStatus.Timeout, Value: null, ModbusExceptionCode: null, NormalizeMessage(message));
    }

    public static ModbusProbeOutcome TransportError(string? message)
    {
        return new ModbusProbeOutcome(ScanResultStatus.TransportError, Value: null, ModbusExceptionCode: null, NormalizeMessage(message));
    }

    public static ModbusProbeOutcome InvalidResponse(string? message)
    {
        return new ModbusProbeOutcome(ScanResultStatus.InvalidResponse, Value: null, ModbusExceptionCode: null, NormalizeMessage(message));
    }

    private static string? NormalizeMessage(string? message)
    {
        return string.IsNullOrWhiteSpace(message) ? null : message;
    }
}
