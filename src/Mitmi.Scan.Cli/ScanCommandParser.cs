using System.Globalization;

namespace Mitmi.Scan.Cli;

public static class ScanCommandParser
{
    private static readonly HashSet<string> KnownOptions = new(StringComparer.Ordinal)
    {
        "--host",
        "--port",
        "--unit-id",
        "--table",
        "--start",
        "--end",
        "--timeout-ms",
        "--delay-ms",
        "--retries",
        "--format",
        "--output"
    };

    private static readonly string[] RequiredOptions =
    [
        "--host",
        "--unit-id",
        "--table",
        "--start",
        "--end"
    ];

    public static CommandParseResult Parse(IReadOnlyList<string> args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (args.Count == 0 || IsHelp(args[0]))
        {
            return CommandParseResult.Help();
        }

        if (!string.Equals(args[0], "scan", StringComparison.Ordinal))
        {
            return CommandParseResult.Invalid([$"Expected command 'scan', but received '{args[0]}'."]);
        }

        if (args.Count == 2 && IsHelp(args[1]))
        {
            return CommandParseResult.Help();
        }

        Dictionary<string, string> values = new(StringComparer.Ordinal);
        List<string> errors = [];

        for (int index = 1; index < args.Count; index++)
        {
            string option = args[index];

            if (!option.StartsWith("--", StringComparison.Ordinal))
            {
                errors.Add($"Unexpected argument '{option}'. Options must use the '--name value' form.");
                continue;
            }

            if (!KnownOptions.Contains(option))
            {
                errors.Add($"Unknown option '{option}'.");
                continue;
            }

            if (values.ContainsKey(option))
            {
                errors.Add($"Option '{option}' cannot be specified more than once.");
            }

            if (index + 1 >= args.Count || args[index + 1].StartsWith("--", StringComparison.Ordinal))
            {
                errors.Add($"Option '{option}' requires a value.");
                continue;
            }

            string value = args[index + 1];
            if (!values.ContainsKey(option))
            {
                values.Add(option, value);
            }

            index++;
        }

        foreach (string requiredOption in RequiredOptions)
        {
            if (!values.ContainsKey(requiredOption))
            {
                errors.Add($"Missing required option '{requiredOption}'.");
            }
        }

        if (errors.Count > 0)
        {
            return CommandParseResult.Invalid(errors);
        }

        string host = values["--host"];
        if (string.IsNullOrWhiteSpace(host))
        {
            errors.Add("Option '--host' must not be empty.");
        }
        else if (host.Contains("://", StringComparison.Ordinal))
        {
            errors.Add("Option '--host' must be a host name or IP address without a URI scheme.");
        }
        else if (ContainsWhiteSpace(host))
        {
            errors.Add("Option '--host' must not contain whitespace.");
        }

        int port = ParseIntegerOption(values, "--port", ScanCommandOptions.DefaultPort, 1, 65_535, errors);
        int unitId = ParseIntegerOption(values, "--unit-id", defaultValue: -1, minimum: 0, maximum: 255, errors: errors);
        int startAddress = ParseIntegerOption(values, "--start", defaultValue: -1, minimum: 0, maximum: 65_535, errors: errors);
        int endAddress = ParseIntegerOption(values, "--end", defaultValue: -1, minimum: 0, maximum: 65_535, errors: errors);
        int timeoutMilliseconds = ParseIntegerOption(values, "--timeout-ms", ScanCommandOptions.DefaultTimeoutMilliseconds, 1, int.MaxValue, errors);
        int delayMilliseconds = ParseIntegerOption(values, "--delay-ms", ScanCommandOptions.DefaultDelayMilliseconds, 0, int.MaxValue, errors);
        int retries = ParseIntegerOption(values, "--retries", ScanCommandOptions.DefaultRetries, 0, int.MaxValue, errors);

        if (startAddress >= 0 && endAddress >= 0 && startAddress > endAddress)
        {
            errors.Add("Option '--start' must be less than or equal to option '--end'.");
        }

        ScanTableSelection table = default;
        if (!ScanTableSelectionExtensions.TryParseCliName(values["--table"], out table))
        {
            errors.Add("Option '--table' must be one of: coils, discrete-inputs, holding-registers, input-registers, all.");
        }

        ReportFormat format = ReportFormat.Console;
        if (values.TryGetValue("--format", out string? formatValue)
            && !ReportFormatExtensions.TryParseCliName(formatValue, out format))
        {
            errors.Add("Option '--format' must be one of: console, csv, markdown.");
        }

        values.TryGetValue("--output", out string? outputPath);
        if (outputPath is not null && string.IsNullOrWhiteSpace(outputPath))
        {
            errors.Add("Option '--output' must not be empty.");
        }

        if ((format == ReportFormat.Csv || format == ReportFormat.Markdown) && outputPath is null)
        {
            errors.Add("Option '--output' is required when '--format' is csv or markdown.");
        }

        if (format == ReportFormat.Console && outputPath is not null)
        {
            errors.Add("Option '--output' is only valid when '--format' is csv or markdown.");
        }

        if (errors.Count > 0)
        {
            return CommandParseResult.Invalid(errors);
        }

        ScanCommandOptions options = new(
            Host: host,
            Port: port,
            UnitId: (byte)unitId,
            Table: table,
            StartAddress: startAddress,
            EndAddress: endAddress,
            TimeoutMilliseconds: timeoutMilliseconds,
            DelayMilliseconds: delayMilliseconds,
            Retries: retries,
            Format: format,
            OutputPath: outputPath);

        return CommandParseResult.Success(options);
    }

    private static bool IsHelp(string value)
    {
        return string.Equals(value, "--help", StringComparison.Ordinal)
            || string.Equals(value, "-h", StringComparison.Ordinal)
            || string.Equals(value, "help", StringComparison.Ordinal);
    }

    private static int ParseIntegerOption(
        IReadOnlyDictionary<string, string> values,
        string option,
        int defaultValue,
        int minimum,
        int maximum,
        List<string> errors)
    {
        if (!values.TryGetValue(option, out string? rawValue))
        {
            return defaultValue;
        }

        if (!int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed)
            || parsed < minimum
            || parsed > maximum)
        {
            errors.Add($"Option '{option}' must be an integer between {minimum.ToString(CultureInfo.InvariantCulture)} and {maximum.ToString(CultureInfo.InvariantCulture)}.");
            return defaultValue;
        }

        return parsed;
    }

    private static bool ContainsWhiteSpace(string value)
    {
        foreach (char character in value)
        {
            if (char.IsWhiteSpace(character))
            {
                return true;
            }
        }

        return false;
    }
}
