using System.Globalization;

namespace Mitmi.Scan.Cli;

public static class ConsoleScanReportRenderer
{
    public static void Write(IEnumerable<ScanResult> results, TextWriter writer)
    {
        ArgumentNullException.ThrowIfNull(results);
        ArgumentNullException.ThrowIfNull(writer);

        List<ScanResult> materializedResults = results.ToList();
        writer.WriteLine("Scan results");
        writer.WriteLine("------------");

        if (materializedResults.Count == 0)
        {
            writer.WriteLine("No results.");
            return;
        }

        writer.WriteLine($"Total: {materializedResults.Count.ToString(CultureInfo.InvariantCulture)}");

        foreach (IGrouping<ScanResultStatus, ScanResult> group in materializedResults
                     .GroupBy(result => result.Status)
                     .OrderBy(group => group.Key))
        {
            writer.WriteLine($"{group.Key.ToReportValue()}: {group.Count().ToString(CultureInfo.InvariantCulture)}");
        }

        writer.WriteLine();

        IReadOnlyList<ScanReportRow> rows = materializedResults
            .OrderBy(result => result.Probe.Table)
            .ThenBy(result => result.Probe.Address)
            .Select(ScanReportRow.FromResult)
            .ToList();

        int[] widths = ScanReportRow.Headers
            .Select((header, index) => Math.Max(header.Length, rows.Max(row => row.Values[index].Length)))
            .ToArray();

        WriteConsoleRow(ScanReportRow.Headers, widths, writer);
        WriteConsoleRow(widths.Select(width => new string('-', width)).ToArray(), widths, writer);

        foreach (ScanReportRow row in rows)
        {
            WriteConsoleRow(row.Values, widths, writer);
        }
    }

    private static void WriteConsoleRow(IReadOnlyList<string> values, IReadOnlyList<int> widths, TextWriter writer)
    {
        for (int index = 0; index < values.Count; index++)
        {
            if (index > 0)
            {
                writer.Write("  ");
            }

            writer.Write(values[index].PadRight(widths[index]));
        }

        writer.WriteLine();
    }
}
