using System.Globalization;

namespace Mitmi.Scan.Cli;

public static class ScanPlanRenderer
{
    public static void Write(ScanCommandOptions options, TextWriter writer)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(writer);

        writer.WriteLine("Dry scan plan");
        writer.WriteLine("-------------");
        writer.WriteLine($"Target: {options.Host}:{options.Port.ToString(CultureInfo.InvariantCulture)}");
        writer.WriteLine($"Unit ID: {options.UnitId.ToString(CultureInfo.InvariantCulture)}");
        writer.WriteLine($"Tables: {options.Table.ToPlanDisplayName()}");
        writer.WriteLine($"Address range: {options.StartAddress.ToString(CultureInfo.InvariantCulture)}..{options.EndAddress.ToString(CultureInfo.InvariantCulture)} ({options.AddressCount.ToString(CultureInfo.InvariantCulture)} addresses)");
        writer.WriteLine($"Planned probes: {options.PlannedProbeCount.ToString(CultureInfo.InvariantCulture)}");
        writer.WriteLine($"Timeout: {options.TimeoutMilliseconds.ToString(CultureInfo.InvariantCulture)} ms");
        writer.WriteLine($"Delay: {options.DelayMilliseconds.ToString(CultureInfo.InvariantCulture)} ms");
        writer.WriteLine($"Retries: {options.Retries.ToString(CultureInfo.InvariantCulture)}");
        writer.WriteLine($"Report: {FormatReport(options)}");
        writer.WriteLine("Network: no connection will be opened in this implementation slice.");
    }

    private static string FormatReport(ScanCommandOptions options)
    {
        if (options.Format == ReportFormat.Console)
        {
            return "console summary";
        }

        return $"{options.Format.ToCliName()} file: {options.OutputPath}";
    }
}
