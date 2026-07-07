using System.Globalization;

namespace Mitmi.Scan.Cli;

public static class MarkdownScanReportRenderer
{
    private static readonly IReadOnlyList<string> ActiveHeaders =
    [
        "Table",
        "Address",
        "Hex",
        "Decimal",
        "ASCII",
        "Binary"
    ];

    public static void Write(ScanRunResult scanRun, TextWriter writer)
    {
        ArgumentNullException.ThrowIfNull(scanRun);
        ArgumentNullException.ThrowIfNull(writer);

        IReadOnlyList<ScanResult> activeResults = scanRun.Results
            .Where(result => result.Status == ScanResultStatus.Readable)
            .OrderBy(result => result.Probe.Table)
            .ThenBy(result => result.Probe.Address)
            .ToList();

        WriteMarkdownRow(ActiveHeaders, writer);
        WriteMarkdownRow(ActiveHeaders.Select(_ => "---").ToArray(), writer);

        foreach (ScanResult result in activeResults)
        {
            WriteMarkdownRow(FormatActiveRow(result), writer);
        }

        writer.WriteLine();
        writer.WriteLine(FormatSummary(scanRun, activeResults.Count));
    }

    private static IReadOnlyList<string> FormatActiveRow(ScanResult result)
    {
        ScanValue value = result.Value ?? throw new InvalidOperationException("Readable scan results must include a value.");

        return
        [
            result.Probe.Table.ToCliName(),
            result.Probe.Address.ToString(CultureInfo.InvariantCulture),
            value.ToHexReportValue(),
            value.ToDecimalReportValue(),
            value.ToAsciiReportValue(),
            value.ToBinaryReportValue()
        ];
    }

    private static string FormatSummary(ScanRunResult scanRun, int activeCount)
    {
        IReadOnlyList<ScanResult> results = scanRun.Results;
        string scannedLabel = FormatScannedLabel(results);
        string activeLabel = FormatActiveLabel(results, activeCount);

        return $"Scanned {results.Count.ToString(CultureInfo.InvariantCulture)} {scannedLabel} in {scanRun.Elapsed.TotalSeconds.ToString("0.###", CultureInfo.InvariantCulture)} seconds; found total of {activeCount.ToString(CultureInfo.InvariantCulture)} active {activeLabel}.";
    }

    private static string FormatScannedLabel(IReadOnlyList<ScanResult> results)
    {
        IReadOnlySet<ModbusTable> tables = results.Select(result => result.Probe.Table).ToHashSet();

        if (tables.Count == 1)
        {
            ModbusTable table = tables.Single();
            return table switch
            {
                ModbusTable.Coils => "coils",
                ModbusTable.DiscreteInputs => "discrete inputs",
                ModbusTable.HoldingRegisters => "holding registers",
                ModbusTable.InputRegisters => "input registers",
                _ => "addresses"
            };
        }

        return "addresses";
    }

    private static string FormatActiveLabel(IReadOnlyList<ScanResult> results, int activeCount)
    {
        IReadOnlySet<ModbusTable> tables = results.Select(result => result.Probe.Table).ToHashSet();
        bool singular = activeCount == 1;

        if (tables.Count == 1)
        {
            ModbusTable table = tables.Single();
            return table switch
            {
                ModbusTable.HoldingRegisters or ModbusTable.InputRegisters => singular ? "register" : "registers",
                ModbusTable.Coils => singular ? "coil" : "coils",
                ModbusTable.DiscreteInputs => singular ? "discrete input" : "discrete inputs",
                _ => "addresses"
            };
        }

        return singular ? "address" : "addresses";
    }

    private static void WriteMarkdownRow(IReadOnlyList<string> values, TextWriter writer)
    {
        writer.Write("| ");
        writer.Write(string.Join(" | ", values.Select(Escape)));
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
