namespace Mitmi.Scan.Cli;

public static class MarkdownScanReportRenderer
{
    public static void Write(IEnumerable<ScanResult> results, TextWriter writer)
    {
        ArgumentNullException.ThrowIfNull(results);
        ArgumentNullException.ThrowIfNull(writer);

        WriteMarkdownRow(ScanReportRow.Headers, writer);
        WriteMarkdownRow(ScanReportRow.Headers.Select(_ => "---").ToArray(), writer);

        foreach (ScanReportRow row in results.Select(ScanReportRow.FromResult))
        {
            WriteMarkdownRow(row.Values.Select(Escape).ToArray(), writer);
        }
    }

    private static void WriteMarkdownRow(IReadOnlyList<string> values, TextWriter writer)
    {
        writer.Write("| ");
        writer.Write(string.Join(" | ", values));
        writer.WriteLine(" |");
    }

    private static string Escape(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("|", "\\|", StringComparison.Ordinal)
            .Replace("\r\n", "<br>", StringComparison.Ordinal)
            .Replace("\n", "<br>", StringComparison.Ordinal)
            .Replace("\r", "<br>", StringComparison.Ordinal);
    }
}
