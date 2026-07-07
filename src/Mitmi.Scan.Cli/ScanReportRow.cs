using System.Globalization;

namespace Mitmi.Scan.Cli;

public sealed record ScanReportRow(
    string Table,
    string UnitId,
    string ZeroBasedAddress,
    string Status,
    string Value,
    string ExceptionCode,
    string Attempts,
    string DurationMilliseconds,
    string Message)
{
    public static readonly IReadOnlyList<string> Headers =
    [
        "Table",
        "Unit ID",
        "Zero-based Address",
        "Status",
        "Value",
        "Exception Code",
        "Attempts",
        "Duration ms",
        "Message"
    ];

    public IReadOnlyList<string> Values =>
    [
        Table,
        UnitId,
        ZeroBasedAddress,
        Status,
        Value,
        ExceptionCode,
        Attempts,
        DurationMilliseconds,
        Message
    ];

    public static ScanReportRow FromResult(ScanResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return new ScanReportRow(
            result.Probe.Table.ToCliName(),
            result.Probe.UnitId.ToString(CultureInfo.InvariantCulture),
            result.Probe.Address.ToString(CultureInfo.InvariantCulture),
            result.Status.ToReportValue(),
            result.Value?.ToReportValue() ?? string.Empty,
            FormatExceptionCode(result.ModbusExceptionCode),
            result.Attempts.ToString(CultureInfo.InvariantCulture),
            result.Duration.TotalMilliseconds.ToString("0.###", CultureInfo.InvariantCulture),
            result.Message ?? string.Empty);
    }

    private static string FormatExceptionCode(byte? exceptionCode)
    {
        return exceptionCode.HasValue
            ? $"0x{exceptionCode.Value.ToString("X2", CultureInfo.InvariantCulture)}"
            : string.Empty;
    }
}
