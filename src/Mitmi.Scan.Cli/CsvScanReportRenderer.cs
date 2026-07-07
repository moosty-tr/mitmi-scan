namespace Mitmi.Scan.Cli;

public static class CsvScanReportRenderer
{
    public static void Write(IEnumerable<ScanResult> results, TextWriter writer)
    {
        ArgumentNullException.ThrowIfNull(results);
        ArgumentNullException.ThrowIfNull(writer);

        WriteCsvLine(ScanReportRow.Headers, writer);

        foreach (ScanReportRow row in results.Select(ScanReportRow.FromResult))
        {
            WriteCsvLine(row.Values, writer);
        }
    }

    private static void WriteCsvLine(IReadOnlyList<string> values, TextWriter writer)
    {
        for (int index = 0; index < values.Count; index++)
        {
            if (index > 0)
            {
                writer.Write(',');
            }

            writer.Write(Escape(values[index]));
        }

        writer.WriteLine();
    }

    private static string Escape(string value)
    {
        if (value.Contains("\"", StringComparison.Ordinal)
            || value.Contains(",", StringComparison.Ordinal)
            || value.Contains("\r", StringComparison.Ordinal)
            || value.Contains("\n", StringComparison.Ordinal))
        {
            return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
        }

        return value;
    }
}
