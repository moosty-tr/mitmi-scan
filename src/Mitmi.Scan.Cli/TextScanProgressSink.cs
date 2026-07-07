namespace Mitmi.Scan.Cli;

public sealed class TextScanProgressSink : IScanProgressSink
{
    private readonly TextWriter _writer;
    private readonly bool _lineOriented;
    private long _nextLineThreshold;
    private long _lineStep;

    public TextScanProgressSink(TextWriter writer, bool lineOriented)
    {
        _writer = writer;
        _lineOriented = lineOriented;
    }

    public void ScanStarted(ScanRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        _lineStep = Math.Max(1, request.PlannedProbeCount / 20);
        _nextLineThreshold = 1;

        if (_lineOriented)
        {
            _writer.WriteLine($"Scan started. Planned probes: {request.PlannedProbeCount}.");
        }
        else
        {
            _writer.Write(ScanProgressFormatter.FormatBar(new ScanProgressSnapshot(0, request.PlannedProbeCount, TimeSpan.Zero)));
        }
    }

    public void ProbeCompleted(ScanProgressSnapshot snapshot)
    {
        if (_lineOriented)
        {
            if (snapshot.CompletedProbes < _nextLineThreshold && snapshot.CompletedProbes < snapshot.TotalProbes)
            {
                return;
            }

            _writer.WriteLine(ScanProgressFormatter.FormatLine(snapshot));
            _nextLineThreshold = snapshot.CompletedProbes + _lineStep;
            return;
        }

        _writer.Write('\r');
        _writer.Write(ScanProgressFormatter.FormatBar(snapshot));
    }

    public void ScanCompleted(ScanProgressSnapshot snapshot)
    {
        if (_lineOriented)
        {
            if (snapshot.CompletedProbes == 0)
            {
                _writer.WriteLine(ScanProgressFormatter.FormatLine(snapshot));
            }

            _writer.WriteLine("Scan completed.");
            return;
        }

        _writer.Write('\r');
        _writer.WriteLine(ScanProgressFormatter.FormatBar(snapshot));
    }
}
