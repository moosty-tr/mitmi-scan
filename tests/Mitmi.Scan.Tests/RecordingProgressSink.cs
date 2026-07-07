using Mitmi.Scan.Cli;

namespace Mitmi.Scan.Tests;

internal sealed class RecordingProgressSink : IScanProgressSink
{
    public int StartedCount { get; private set; }

    public int CompletedCount { get; private set; }

    public List<ScanProgressSnapshot> ProbeSnapshots { get; } = [];

    public ScanProgressSnapshot? FinalSnapshot { get; private set; }

    public void ScanStarted(ScanRequest request)
    {
        _ = request;
        StartedCount++;
    }

    public void ProbeCompleted(ScanProgressSnapshot snapshot)
    {
        ProbeSnapshots.Add(snapshot);
    }

    public void ScanCompleted(ScanProgressSnapshot snapshot)
    {
        CompletedCount++;
        FinalSnapshot = snapshot;
    }
}
