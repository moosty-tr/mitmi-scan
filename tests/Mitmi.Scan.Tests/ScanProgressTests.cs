using System.Globalization;
using Mitmi.Scan.Cli;

namespace Mitmi.Scan.Tests;

public sealed class ScanProgressTests
{
    [Fact]
    public void Snapshot_CalculatesPercentAndEtaFromObservedAverage()
    {
        ScanProgressSnapshot snapshot = new(
            completedProbes: 5,
            totalProbes: 10,
            elapsed: TimeSpan.FromSeconds(10));

        Assert.Equal(50.0, snapshot.PercentComplete);
        Assert.Equal(TimeSpan.FromSeconds(10), snapshot.EstimatedRemaining);
    }

    [Fact]
    public void Snapshot_BeforeFirstProbe_HasUnknownEta()
    {
        ScanProgressSnapshot snapshot = new(
            completedProbes: 0,
            totalProbes: 10,
            elapsed: TimeSpan.Zero);

        Assert.Null(snapshot.EstimatedRemaining);
    }

    [Fact]
    public void Formatter_RendersAsciiProgressBar()
    {
        ScanProgressSnapshot snapshot = new(
            completedProbes: 5,
            totalProbes: 10,
            elapsed: TimeSpan.FromSeconds(10));

        string formatted = ScanProgressFormatter.FormatBar(snapshot);

        Assert.Equal("[##########----------] 50.0% 5/10 ETA 00:00:10", formatted);
    }

    [Fact]
    public void TextSink_WhenLineOriented_WritesReadableProgressLines()
    {
        ScanRequest request = new(
            host: "plc.local",
            port: 502,
            unitId: 1,
            tables: [ModbusTable.HoldingRegisters],
            startAddress: 0,
            endAddress: 1,
            timeoutMilliseconds: 1_000,
            delayMilliseconds: 0,
            retries: 0);
        using StringWriter writer = new(CultureInfo.InvariantCulture);
        TextScanProgressSink sink = new(writer, lineOriented: true);

        sink.ScanStarted(request);
        sink.ProbeCompleted(new ScanProgressSnapshot(1, 2, TimeSpan.FromSeconds(1)));
        sink.ProbeCompleted(new ScanProgressSnapshot(2, 2, TimeSpan.FromSeconds(2)));
        sink.ScanCompleted(new ScanProgressSnapshot(2, 2, TimeSpan.FromSeconds(2)));

        string output = writer.ToString();
        Assert.Contains("Scan started. Planned probes: 2.", output, StringComparison.Ordinal);
        Assert.Contains("Progress: [##########----------] 50.0% 1/2 ETA 00:00:01", output, StringComparison.Ordinal);
        Assert.Contains("Progress: [####################] 100.0% 2/2 ETA 00:00:00", output, StringComparison.Ordinal);
        Assert.Contains("Scan completed.", output, StringComparison.Ordinal);
    }
}
