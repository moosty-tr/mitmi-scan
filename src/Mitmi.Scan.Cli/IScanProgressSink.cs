namespace Mitmi.Scan.Cli;

public interface IScanProgressSink
{
    void ScanStarted(ScanRequest request);

    void ProbeCompleted(ScanProgressSnapshot snapshot);

    void ScanCompleted(ScanProgressSnapshot snapshot);
}
