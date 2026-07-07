namespace Mitmi.Scan.Cli;

public sealed class NoOpScanProgressSink : IScanProgressSink
{
    public static readonly NoOpScanProgressSink Instance = new();

    private NoOpScanProgressSink()
    {
    }

    public void ScanStarted(ScanRequest request)
    {
        _ = request;
    }

    public void ProbeCompleted(ScanProgressSnapshot snapshot)
    {
        _ = snapshot;
    }

    public void ScanCompleted(ScanProgressSnapshot snapshot)
    {
        _ = snapshot;
    }
}
