namespace Mitmi.Scan.Cli;

public sealed record ScanRunResult
{
    public ScanRunResult(IReadOnlyList<ScanResult> results, TimeSpan elapsed)
    {
        ArgumentNullException.ThrowIfNull(results);

        if (elapsed < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(elapsed), elapsed, "Elapsed time must not be negative.");
        }

        Results = results;
        Elapsed = elapsed;
    }

    public IReadOnlyList<ScanResult> Results { get; }

    public TimeSpan Elapsed { get; }
}
