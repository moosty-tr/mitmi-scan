namespace Mitmi.Scan.Cli;

public enum ReportFormat
{
    Console,
    Csv,
    Markdown
}

public static class ReportFormatExtensions
{
    public static bool TryParseCliName(string value, out ReportFormat format)
    {
        switch (value)
        {
            case "console":
                format = ReportFormat.Console;
                return true;
            case "csv":
                format = ReportFormat.Csv;
                return true;
            case "markdown":
                format = ReportFormat.Markdown;
                return true;
            default:
                format = default;
                return false;
        }
    }

    public static string ToCliName(this ReportFormat format)
    {
        return format switch
        {
            ReportFormat.Console => "console",
            ReportFormat.Csv => "csv",
            ReportFormat.Markdown => "markdown",
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unknown report format.")
        };
    }
}
