namespace Mitmi.Scan.Cli;

public sealed record ScanResult
{
    private ScanResult(
        ScanProbe probe,
        ScanResultStatus status,
        ScanValue? value,
        byte? modbusExceptionCode,
        int attempts,
        TimeSpan duration,
        DateTimeOffset timestampUtc,
        string? message)
    {
        ArgumentNullException.ThrowIfNull(probe);
        ArgumentOutOfRangeException.ThrowIfLessThan(attempts, 1);

        if (duration < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), duration, "Duration must not be negative.");
        }

        Probe = probe;
        Status = status;
        Value = value;
        ModbusExceptionCode = modbusExceptionCode;
        Attempts = attempts;
        Duration = duration;
        TimestampUtc = timestampUtc.ToUniversalTime();
        Message = string.IsNullOrWhiteSpace(message) ? null : message;
    }

    public ScanProbe Probe { get; }

    public ScanResultStatus Status { get; }

    public ScanValue? Value { get; }

    public byte? ModbusExceptionCode { get; }

    public int Attempts { get; }

    public TimeSpan Duration { get; }

    public DateTimeOffset TimestampUtc { get; }

    public string? Message { get; }

    public static ScanResult Readable(
        ScanProbe probe,
        ScanValue value,
        int attempts,
        TimeSpan duration,
        DateTimeOffset timestampUtc,
        string? message = null)
    {
        ArgumentNullException.ThrowIfNull(value);

        return new ScanResult(probe, ScanResultStatus.Readable, value, modbusExceptionCode: null, attempts, duration, timestampUtc, message);
    }

    public static ScanResult ModbusException(
        ScanProbe probe,
        byte exceptionCode,
        int attempts,
        TimeSpan duration,
        DateTimeOffset timestampUtc,
        string? message = null)
    {
        return new ScanResult(probe, ScanResultStatus.ModbusException, value: null, exceptionCode, attempts, duration, timestampUtc, message);
    }

    public static ScanResult Timeout(
        ScanProbe probe,
        int attempts,
        TimeSpan duration,
        DateTimeOffset timestampUtc,
        string? message = null)
    {
        return new ScanResult(probe, ScanResultStatus.Timeout, value: null, modbusExceptionCode: null, attempts, duration, timestampUtc, message);
    }

    public static ScanResult TransportError(
        ScanProbe probe,
        int attempts,
        TimeSpan duration,
        DateTimeOffset timestampUtc,
        string? message = null)
    {
        return new ScanResult(probe, ScanResultStatus.TransportError, value: null, modbusExceptionCode: null, attempts, duration, timestampUtc, message);
    }

    public static ScanResult InvalidResponse(
        ScanProbe probe,
        int attempts,
        TimeSpan duration,
        DateTimeOffset timestampUtc,
        string? message = null)
    {
        return new ScanResult(probe, ScanResultStatus.InvalidResponse, value: null, modbusExceptionCode: null, attempts, duration, timestampUtc, message);
    }
}
