namespace Mitmi.Scan.Cli;

public enum ScanResultStatus
{
    Readable,
    ModbusException,
    Timeout,
    TransportError,
    InvalidResponse
}

public static class ScanResultStatusExtensions
{
    public static string ToReportValue(this ScanResultStatus status)
    {
        return status switch
        {
            ScanResultStatus.Readable => "readable",
            ScanResultStatus.ModbusException => "modbus-exception",
            ScanResultStatus.Timeout => "timeout",
            ScanResultStatus.TransportError => "transport-error",
            ScanResultStatus.InvalidResponse => "invalid-response",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown scan result status.")
        };
    }
}
