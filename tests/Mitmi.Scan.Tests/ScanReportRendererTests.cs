using System.Globalization;
using Mitmi.Scan.Cli;

namespace Mitmi.Scan.Tests;

public sealed class ScanReportRendererTests
{
    [Fact]
    public void ReportRowHeaders_RemainStable()
    {
        Assert.Equal(
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
            ],
            ScanReportRow.Headers);
    }

    [Fact]
    public void ReportRow_FromReadableRegister_UsesCanonicalValues()
    {
        ScanResult result = ScanResult.Readable(
            Probe(ModbusTable.HoldingRegisters, address: 12),
            ScanValue.Register(42),
            attempts: 2,
            duration: TimeSpan.FromMilliseconds(15.25),
            timestampUtc: DateTimeOffset.UnixEpoch);

        ScanReportRow row = ScanReportRow.FromResult(result);

        Assert.Equal(
            [
                "holding-registers",
                "1",
                "12",
                "readable",
                "42",
                string.Empty,
                "2",
                "15.25",
                string.Empty
            ],
            row.Values);
    }

    [Fact]
    public void ReportRow_FromModbusException_FormatsExceptionCode()
    {
        ScanResult result = ScanResult.ModbusException(
            Probe(ModbusTable.InputRegisters, address: 14),
            exceptionCode: 0x02,
            attempts: 1,
            duration: TimeSpan.FromMilliseconds(9),
            timestampUtc: DateTimeOffset.UnixEpoch,
            message: "illegal data address");

        ScanReportRow row = ScanReportRow.FromResult(result);

        Assert.Equal("modbus-exception", row.Status);
        Assert.Equal("0x02", row.ExceptionCode);
        Assert.Equal("illegal data address", row.Message);
    }

    [Fact]
    public void CsvRenderer_WritesStableColumnsAndEscapesFields()
    {
        ScanResult readable = ScanResult.Readable(
            Probe(ModbusTable.Coils, address: 0),
            ScanValue.Bit(true),
            attempts: 1,
            duration: TimeSpan.FromMilliseconds(1),
            timestampUtc: DateTimeOffset.UnixEpoch);
        ScanResult transportError = ScanResult.TransportError(
            Probe(ModbusTable.Coils, address: 1),
            attempts: 2,
            duration: TimeSpan.FromMilliseconds(1000),
            timestampUtc: DateTimeOffset.UnixEpoch,
            message: "transport \"closed\", retry\nlater");

        using StringWriter writer = new(CultureInfo.InvariantCulture);

        CsvScanReportRenderer.Write([readable, transportError], writer);

        string expected = string.Join(
            Environment.NewLine,
            [
                "Table,Unit ID,Zero-based Address,Status,Value,Exception Code,Attempts,Duration ms,Message",
                "coils,1,0,readable,true,,1,1,",
                "coils,1,1,transport-error,,,2,1000,\"transport \"\"closed\"\", retry\nlater\""
            ])
            + Environment.NewLine;

        Assert.Equal(expected, writer.ToString());
    }

    [Fact]
    public void MarkdownRenderer_WritesStableColumnsAndEscapesPipeAndNewline()
    {
        ScanResult result = ScanResult.InvalidResponse(
            Probe(ModbusTable.DiscreteInputs, address: 7),
            attempts: 1,
            duration: TimeSpan.FromMilliseconds(3),
            timestampUtc: DateTimeOffset.UnixEpoch,
            message: "bad | frame\r\nretry");

        using StringWriter writer = new(CultureInfo.InvariantCulture);

        MarkdownScanReportRenderer.Write([result], writer);

        string expected = string.Join(
            Environment.NewLine,
            [
                "| Table | Unit ID | Zero-based Address | Status | Value | Exception Code | Attempts | Duration ms | Message |",
                "| --- | --- | --- | --- | --- | --- | --- | --- | --- |",
                "| discrete-inputs | 1 | 7 | invalid-response |  |  | 1 | 3 | bad \\| frame<br>retry |"
            ])
            + Environment.NewLine;

        Assert.Equal(expected, writer.ToString());
    }

    [Fact]
    public void ConsoleRenderer_WritesSummaryAndRows()
    {
        ScanResult readable = ScanResult.Readable(
            Probe(ModbusTable.HoldingRegisters, address: 2),
            ScanValue.Register(100),
            attempts: 1,
            duration: TimeSpan.FromMilliseconds(5),
            timestampUtc: DateTimeOffset.UnixEpoch);
        ScanResult timeout = ScanResult.Timeout(
            Probe(ModbusTable.HoldingRegisters, address: 3),
            attempts: 1,
            duration: TimeSpan.FromMilliseconds(1000),
            timestampUtc: DateTimeOffset.UnixEpoch,
            message: "request timed out");

        using StringWriter writer = new(CultureInfo.InvariantCulture);

        ConsoleScanReportRenderer.Write([readable, timeout], writer);

        string output = writer.ToString();
        Assert.Contains("Scan results", output, StringComparison.Ordinal);
        Assert.Contains("Total: 2", output, StringComparison.Ordinal);
        Assert.Contains("readable: 1", output, StringComparison.Ordinal);
        Assert.Contains("timeout: 1", output, StringComparison.Ordinal);
        Assert.Contains("holding-registers", output, StringComparison.Ordinal);
        Assert.Contains("request timed out", output, StringComparison.Ordinal);
    }

    private static ScanProbe Probe(ModbusTable table, int address)
    {
        return new ScanProbe("plc.local", 502, 1, table, address);
    }
}
