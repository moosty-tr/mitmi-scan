using System.Globalization;

namespace Mitmi.Scan.Cli;

public static class ScanProgressFormatter
{
    public const int DefaultBarWidth = 20;

    public static string FormatBar(ScanProgressSnapshot snapshot, int width = DefaultBarWidth)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(width, 1);

        int filled = snapshot.TotalProbes == 0
            ? width
            : (int)Math.Floor(snapshot.PercentComplete / 100.0 * width);
        filled = Math.Clamp(filled, 0, width);

        string bar = new string('#', filled) + new string('-', width - filled);
        return $"[{bar}] {snapshot.PercentComplete.ToString("0.0", CultureInfo.InvariantCulture)}% {snapshot.CompletedProbes.ToString(CultureInfo.InvariantCulture)}/{snapshot.TotalProbes.ToString(CultureInfo.InvariantCulture)} ETA {FormatDuration(snapshot.EstimatedRemaining)}";
    }

    public static string FormatLine(ScanProgressSnapshot snapshot)
    {
        return $"Progress: {FormatBar(snapshot)}";
    }

    private static string FormatDuration(TimeSpan? duration)
    {
        if (duration is null)
        {
            return "--:--:--";
        }

        TimeSpan normalized = duration.Value < TimeSpan.Zero ? TimeSpan.Zero : duration.Value;
        int totalHours = checked((int)Math.Floor(normalized.TotalHours));
        return string.Create(CultureInfo.InvariantCulture, $"{totalHours:00}:{normalized.Minutes:00}:{normalized.Seconds:00}");
    }
}
