using Mitmi.Scan.Cli;

namespace Mitmi.Scan.Tests;

public sealed class ScanModelTests
{
    [Fact]
    public void FromOptions_WithAllTables_ExpandsToConcreteTablesInStableOrder()
    {
        ScanCommandOptions options = new(
            Host: "plc.local",
            Port: 502,
            UnitId: 1,
            Table: ScanTableSelection.All,
            StartAddress: 0,
            EndAddress: 2,
            TimeoutMilliseconds: 1_000,
            DelayMilliseconds: 10,
            Retries: 0,
            Format: ReportFormat.Console,
            OutputPath: null);

        ScanRequest request = ScanRequest.FromOptions(options);

        Assert.Equal(
            [
                ModbusTable.Coils,
                ModbusTable.DiscreteInputs,
                ModbusTable.HoldingRegisters,
                ModbusTable.InputRegisters
            ],
            request.Tables);
        Assert.Equal(3, request.AddressCount);
        Assert.Equal(12, request.PlannedProbeCount);
    }

    [Fact]
    public void ReadableResult_StoresValueAndNormalizesTimestampToUtc()
    {
        ScanProbe probe = new("plc.local", 502, 1, ModbusTable.HoldingRegisters, 10);
        DateTimeOffset timestampWithOffset = new(2026, 7, 7, 9, 30, 0, TimeSpan.FromHours(3));

        ScanResult result = ScanResult.Readable(
            probe,
            ScanValue.Register(1234),
            attempts: 1,
            duration: TimeSpan.FromMilliseconds(12.5),
            timestampWithOffset,
            message: "observed");

        Assert.Equal(ScanResultStatus.Readable, result.Status);
        Assert.Equal("1234", result.Value?.ToReportValue());
        Assert.Null(result.ModbusExceptionCode);
        Assert.Equal(TimeSpan.Zero, result.TimestampUtc.Offset);
        Assert.Equal(6, result.TimestampUtc.Hour);
    }

    [Fact]
    public void ModbusExceptionResult_StoresExceptionCodeAsProtocolOutcome()
    {
        ScanProbe probe = new("plc.local", 502, 1, ModbusTable.InputRegisters, 11);

        ScanResult result = ScanResult.ModbusException(
            probe,
            exceptionCode: 0x02,
            attempts: 1,
            duration: TimeSpan.FromMilliseconds(8),
            timestampUtc: DateTimeOffset.UnixEpoch,
            message: "illegal data address");

        Assert.Equal(ScanResultStatus.ModbusException, result.Status);
        Assert.Null(result.Value);
        Assert.Equal((byte)0x02, result.ModbusExceptionCode);
    }

    [Fact]
    public void ResultFactory_WithZeroAttempts_Throws()
    {
        ScanProbe probe = new("plc.local", 502, 1, ModbusTable.Coils, 0);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ScanResult.Timeout(
                probe,
                attempts: 0,
                duration: TimeSpan.FromMilliseconds(1),
                timestampUtc: DateTimeOffset.UnixEpoch));
    }
}
