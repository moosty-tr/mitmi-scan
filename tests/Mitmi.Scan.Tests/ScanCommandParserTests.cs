using Mitmi.Scan.Cli;

namespace Mitmi.Scan.Tests;

public sealed class ScanCommandParserTests
{
    [Fact]
    public void Parse_WithMinimalValidScanCommand_UsesExpectedDefaults()
    {
        CommandParseResult result = ScanCommandParser.Parse(
        [
            "scan",
            "--host", "plc.local",
            "--unit-id", "1",
            "--table", "holding-registers",
            "--start", "0",
            "--end", "9"
        ]);

        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Errors));

        ScanCommandOptions options = Assert.IsType<ScanCommandOptions>(result.Options);
        Assert.Equal("plc.local", options.Host);
        Assert.Equal(502, options.Port);
        Assert.Equal((byte)1, options.UnitId);
        Assert.Equal(ScanTableSelection.HoldingRegisters, options.Table);
        Assert.Equal(0, options.StartAddress);
        Assert.Equal(9, options.EndAddress);
        Assert.Equal(1_000, options.TimeoutMilliseconds);
        Assert.Equal(10, options.DelayMilliseconds);
        Assert.Equal(0, options.Retries);
        Assert.Equal(ReportFormat.Console, options.Format);
        Assert.Null(options.OutputPath);
        Assert.Equal(10, options.AddressCount);
        Assert.Equal(10, options.PlannedProbeCount);
    }

    [Fact]
    public void Parse_WithAllTables_MultipliesProbeCountByFour()
    {
        CommandParseResult result = ScanCommandParser.Parse(
        [
            "scan",
            "--host", "plc.local",
            "--unit-id", "1",
            "--table", "all",
            "--start", "5",
            "--end", "7"
        ]);

        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Errors));
        ScanCommandOptions options = Assert.IsType<ScanCommandOptions>(result.Options);
        Assert.Equal(3, options.AddressCount);
        Assert.Equal(12, options.PlannedProbeCount);
    }

    [Fact]
    public void Parse_WithExplicitOptions_OverridesDefaults()
    {
        CommandParseResult result = ScanCommandParser.Parse(
        [
            "scan",
            "--host", "192.168.1.50",
            "--port", "1502",
            "--unit-id", "255",
            "--table", "coils",
            "--start", "10",
            "--end", "10",
            "--timeout-ms", "2500",
            "--delay-ms", "0",
            "--retries", "2",
            "--format", "csv",
            "--output", "scan.csv"
        ]);

        Assert.True(result.Succeeded, string.Join(Environment.NewLine, result.Errors));
        ScanCommandOptions options = Assert.IsType<ScanCommandOptions>(result.Options);
        Assert.Equal(1502, options.Port);
        Assert.Equal((byte)255, options.UnitId);
        Assert.Equal(ScanTableSelection.Coils, options.Table);
        Assert.Equal(2_500, options.TimeoutMilliseconds);
        Assert.Equal(0, options.DelayMilliseconds);
        Assert.Equal(2, options.Retries);
        Assert.Equal(ReportFormat.Csv, options.Format);
        Assert.Equal("scan.csv", options.OutputPath);
    }

    [Fact]
    public void Parse_WhenRequiredOptionIsMissing_ReturnsValidationError()
    {
        CommandParseResult result = ScanCommandParser.Parse(
        [
            "scan",
            "--host", "plc.local",
            "--unit-id", "1",
            "--start", "0",
            "--end", "9"
        ]);

        Assert.False(result.Succeeded);
        Assert.Contains("Missing required option '--table'.", result.Errors);
    }

    [Theory]
    [InlineData("--port", "0", "Option '--port' must be an integer between 1 and 65535.")]
    [InlineData("--unit-id", "256", "Option '--unit-id' must be an integer between 0 and 255.")]
    [InlineData("--start", "-1", "Option '--start' must be an integer between 0 and 65535.")]
    [InlineData("--end", "65536", "Option '--end' must be an integer between 0 and 65535.")]
    [InlineData("--timeout-ms", "0", "Option '--timeout-ms' must be an integer between 1 and 2147483647.")]
    [InlineData("--delay-ms", "-1", "Option '--delay-ms' must be an integer between 0 and 2147483647.")]
    [InlineData("--retries", "-1", "Option '--retries' must be an integer between 0 and 2147483647.")]
    public void Parse_WithOutOfRangeIntegerOption_ReturnsValidationError(string option, string value, string expectedError)
    {
        List<string> args =
        [
            "scan",
            "--host", "plc.local",
            "--table", "holding-registers",
        ];

        AddOption(args, "--unit-id", option, value, "1");
        AddOption(args, "--start", option, value, "0");
        AddOption(args, "--end", option, value, "9");

        if (option is "--port" or "--timeout-ms" or "--delay-ms" or "--retries")
        {
            args.Add(option);
            args.Add(value);
        }

        CommandParseResult result = ScanCommandParser.Parse(args);

        Assert.False(result.Succeeded);
        Assert.Contains(expectedError, result.Errors);
    }

    [Fact]
    public void Parse_WhenStartIsGreaterThanEnd_ReturnsValidationError()
    {
        CommandParseResult result = ScanCommandParser.Parse(
        [
            "scan",
            "--host", "plc.local",
            "--unit-id", "1",
            "--table", "holding-registers",
            "--start", "10",
            "--end", "9"
        ]);

        Assert.False(result.Succeeded);
        Assert.Contains("Option '--start' must be less than or equal to option '--end'.", result.Errors);
    }

    [Fact]
    public void Parse_WithUnknownTable_ReturnsValidationError()
    {
        CommandParseResult result = ScanCommandParser.Parse(
        [
            "scan",
            "--host", "plc.local",
            "--unit-id", "1",
            "--table", "registers",
            "--start", "0",
            "--end", "9"
        ]);

        Assert.False(result.Succeeded);
        Assert.Contains("Option '--table' must be one of: coils, discrete-inputs, holding-registers, input-registers, all.", result.Errors);
    }

    [Fact]
    public void Parse_WithCsvFormatAndNoOutput_ReturnsValidationError()
    {
        CommandParseResult result = ScanCommandParser.Parse(
        [
            "scan",
            "--host", "plc.local",
            "--unit-id", "1",
            "--table", "holding-registers",
            "--start", "0",
            "--end", "9",
            "--format", "csv"
        ]);

        Assert.False(result.Succeeded);
        Assert.Contains("Option '--output' is required when '--format' is csv or markdown.", result.Errors);
    }

    [Fact]
    public void Parse_WithConsoleFormatAndOutput_ReturnsValidationError()
    {
        CommandParseResult result = ScanCommandParser.Parse(
        [
            "scan",
            "--host", "plc.local",
            "--unit-id", "1",
            "--table", "holding-registers",
            "--start", "0",
            "--end", "9",
            "--output", "scan.txt"
        ]);

        Assert.False(result.Succeeded);
        Assert.Contains("Option '--output' is only valid when '--format' is csv or markdown.", result.Errors);
    }

    [Fact]
    public void Parse_WithDuplicateOption_ReturnsValidationError()
    {
        CommandParseResult result = ScanCommandParser.Parse(
        [
            "scan",
            "--host", "plc-a.local",
            "--host", "plc-b.local",
            "--unit-id", "1",
            "--table", "holding-registers",
            "--start", "0",
            "--end", "9"
        ]);

        Assert.False(result.Succeeded);
        Assert.Contains("Option '--host' cannot be specified more than once.", result.Errors);
    }

    [Fact]
    public void Parse_WithMissingOptionValue_ReturnsValidationError()
    {
        CommandParseResult result = ScanCommandParser.Parse(
        [
            "scan",
            "--host", "plc.local",
            "--unit-id", "1",
            "--table", "holding-registers",
            "--start", "0",
            "--end"
        ]);

        Assert.False(result.Succeeded);
        Assert.Contains("Option '--end' requires a value.", result.Errors);
    }

    private static void AddOption(List<string> args, string optionName, string testedOption, string testedValue, string defaultValue)
    {
        args.Add(optionName);
        args.Add(optionName == testedOption ? testedValue : defaultValue);
    }
}
