using System.Text;

namespace Mitmi.Scan.Cli;

public static class ScanReportWriter
{
    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    public static async Task WriteAsync(
        ReportFormat format,
        string? outputPath,
        IReadOnlyList<ScanResult> results,
        TextWriter consoleOutput,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(results);
        ArgumentNullException.ThrowIfNull(consoleOutput);

        if (format == ReportFormat.Console)
        {
            ConsoleScanReportRenderer.Write(results, consoleOutput);
            return;
        }

        if (outputPath is null)
        {
            throw new InvalidOperationException("File-oriented reports require an output path.");
        }

        await using FileStream stream = new(outputPath, FileMode.Create, FileAccess.Write, FileShare.Read);
        await using StreamWriter writer = new(stream, Utf8NoBom);

        WriteReport(format, results, writer);
        await writer.FlushAsync(cancellationToken).ConfigureAwait(false);

        consoleOutput.WriteLine($"Scan completed. Results: {results.Count}. Report: {outputPath}");
    }

    private static void WriteReport(ReportFormat format, IReadOnlyList<ScanResult> results, TextWriter writer)
    {
        switch (format)
        {
            case ReportFormat.Csv:
                CsvScanReportRenderer.Write(results, writer);
                break;
            case ReportFormat.Markdown:
                MarkdownScanReportRenderer.Write(results, writer);
                break;
            case ReportFormat.Console:
                ConsoleScanReportRenderer.Write(results, writer);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(format), format, "Unknown report format.");
        }
    }
}
