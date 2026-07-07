namespace Mitmi.Scan.Cli;

public sealed record ScanProgressSnapshot
{
    public ScanProgressSnapshot(long completedProbes, long totalProbes, TimeSpan elapsed)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(completedProbes, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(totalProbes, 0);

        if (completedProbes > totalProbes)
        {
            throw new ArgumentException("Completed probe count cannot exceed total probe count.", nameof(completedProbes));
        }

        if (elapsed < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(elapsed), elapsed, "Elapsed time must not be negative.");
        }

        CompletedProbes = completedProbes;
        TotalProbes = totalProbes;
        Elapsed = elapsed;
    }

    public long CompletedProbes { get; }

    public long TotalProbes { get; }

    public TimeSpan Elapsed { get; }

    public double PercentComplete => TotalProbes == 0 ? 100.0 : CompletedProbes / (double)TotalProbes * 100.0;

    public TimeSpan? EstimatedRemaining
    {
        get
        {
            if (CompletedProbes == 0 || TotalProbes == 0 || CompletedProbes >= TotalProbes)
            {
                return CompletedProbes >= TotalProbes ? TimeSpan.Zero : null;
            }

            double averageTicksPerProbe = Elapsed.Ticks / (double)CompletedProbes;
            double remainingTicks = averageTicksPerProbe * (TotalProbes - CompletedProbes);
            return TimeSpan.FromTicks(checked((long)Math.Round(remainingTicks)));
        }
    }
}
