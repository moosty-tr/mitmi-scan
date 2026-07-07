using Mitmi.Scan.Cli;

namespace Mitmi.Scan.Tests;

public sealed class ScanRunnerTests
{
    [Fact]
    public async Task RunAsync_ProbesTablesThenAddressesInDeterministicOrder()
    {
        ScanRequest request = new(
            host: "plc.local",
            port: 502,
            unitId: 1,
            tables: [ModbusTable.Coils, ModbusTable.HoldingRegisters],
            startAddress: 0,
            endAddress: 1,
            timeoutMilliseconds: 1_000,
            delayMilliseconds: 0,
            retries: 0);
        ScriptedAddressProbeClient client = new(ModbusProbeOutcome.Readable(ScanValue.Bit(true)));
        ScanRunner runner = new(new SingleClientFactory(client));

        ScanRunResult scanRun = await runner.RunAsync(request, CancellationToken.None);
        IReadOnlyList<ScanResult> results = scanRun.Results;

        Assert.Equal(4, results.Count);
        Assert.True(scanRun.Elapsed >= TimeSpan.Zero);
        Assert.Equal(
            [
                (ModbusTable.Coils, 0),
                (ModbusTable.Coils, 1),
                (ModbusTable.HoldingRegisters, 0),
                (ModbusTable.HoldingRegisters, 1)
            ],
            client.Probes.Select(probe => (probe.Table, probe.Address)).ToArray());
    }

    [Fact]
    public async Task RunAsync_RetriesTimeoutAndReconnectsBeforeRetry()
    {
        ScanRequest request = Request(retries: 1);
        ScriptedAddressProbeClient client = new(
            ModbusProbeOutcome.Timeout("slow"),
            ModbusProbeOutcome.Readable(ScanValue.Register(12)));
        ScanRunner runner = new(new SingleClientFactory(client));

        ScanRunResult scanRun = await runner.RunAsync(request, CancellationToken.None);
        IReadOnlyList<ScanResult> results = scanRun.Results;

        ScanResult result = Assert.Single(results);
        Assert.Equal(ScanResultStatus.Readable, result.Status);
        Assert.Equal(2, result.Attempts);
        Assert.Equal(1, client.ReconnectCount);
    }

    [Fact]
    public async Task RunAsync_DoesNotRetryModbusException()
    {
        ScanRequest request = Request(retries: 1);
        ScriptedAddressProbeClient client = new(
            ModbusProbeOutcome.ModbusException(0x02, "illegal data address"),
            ModbusProbeOutcome.Readable(ScanValue.Register(12)));
        ScanRunner runner = new(new SingleClientFactory(client));

        ScanRunResult scanRun = await runner.RunAsync(request, CancellationToken.None);
        IReadOnlyList<ScanResult> results = scanRun.Results;

        ScanResult result = Assert.Single(results);
        Assert.Equal(ScanResultStatus.ModbusException, result.Status);
        Assert.Equal(1, result.Attempts);
        Assert.Equal(0, client.ReconnectCount);
    }

    [Fact]
    public async Task RunAsync_RecordsTransportErrorAfterRetryBudgetIsExhausted()
    {
        ScanRequest request = Request(retries: 1);
        ScriptedAddressProbeClient client = new(
            ModbusProbeOutcome.TransportError("socket closed"),
            ModbusProbeOutcome.TransportError("socket still closed"));
        ScanRunner runner = new(new SingleClientFactory(client));

        ScanRunResult scanRun = await runner.RunAsync(request, CancellationToken.None);
        IReadOnlyList<ScanResult> results = scanRun.Results;

        ScanResult result = Assert.Single(results);
        Assert.Equal(ScanResultStatus.TransportError, result.Status);
        Assert.Equal(2, result.Attempts);
        Assert.Equal(2, client.ReconnectCount);
    }

    [Fact]
    public async Task RunAsync_EmitsProgressSnapshots()
    {
        ScanRequest request = new(
            host: "plc.local",
            port: 502,
            unitId: 1,
            tables: [ModbusTable.HoldingRegisters],
            startAddress: 0,
            endAddress: 2,
            timeoutMilliseconds: 1_000,
            delayMilliseconds: 0,
            retries: 0);
        ScriptedAddressProbeClient client = new(ModbusProbeOutcome.Readable(ScanValue.Register(1)));
        RecordingProgressSink progress = new();
        ScanRunner runner = new(new SingleClientFactory(client));

        _ = await runner.RunAsync(request, progress, CancellationToken.None);

        Assert.Equal(1, progress.StartedCount);
        Assert.Equal(1, progress.CompletedCount);
        Assert.Equal([1, 2, 3], progress.ProbeSnapshots.Select(snapshot => snapshot.CompletedProbes).ToArray());
        Assert.Equal(3, progress.FinalSnapshot?.CompletedProbes);
        Assert.Equal(3, progress.FinalSnapshot?.TotalProbes);
    }

    private static ScanRequest Request(int retries)
    {
        return new ScanRequest(
            host: "plc.local",
            port: 502,
            unitId: 1,
            tables: [ModbusTable.HoldingRegisters],
            startAddress: 0,
            endAddress: 0,
            timeoutMilliseconds: 1_000,
            delayMilliseconds: 0,
            retries: retries);
    }
}
