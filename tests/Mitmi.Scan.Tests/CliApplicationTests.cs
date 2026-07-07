using System.Globalization;
using Mitmi.Scan.Cli;

namespace Mitmi.Scan.Tests;

public sealed class CliApplicationTests
{
    [Fact]
    public async Task RunAsync_WithValidScanCommand_WritesConsoleReportAndReturnsSuccess()
    {
        string[] args =
        [
            "scan",
            "--host", "192.168.1.50",
            "--unit-id", "1",
            "--table", "holding-registers",
            "--start", "0",
            "--end", "1"
        ];
        ScriptedAddressProbeClient client = new(ModbusProbeOutcome.Readable(ScanValue.Register(7)));
        using StringWriter output = new(CultureInfo.InvariantCulture);
        using StringWriter error = new(CultureInfo.InvariantCulture);

        int exitCode = await CliApplication.RunAsync(
            args,
            output,
            error,
            new SingleClientFactory(client));

        Assert.Equal(CliExitCodes.Success, exitCode);
        Assert.Contains("Scan results", output.ToString(), StringComparison.Ordinal);
        Assert.Contains("Total: 2", output.ToString(), StringComparison.Ordinal);
        Assert.Contains("readable: 2", output.ToString(), StringComparison.Ordinal);
        Assert.Equal(string.Empty, error.ToString());
        Assert.Equal(1, client.ConnectCount);
    }

    [Fact]
    public async Task RunAsync_WithCsvOutput_WritesReportFile()
    {
        string outputPath = Path.Combine(Path.GetTempPath(), $"mitmi-scan-{Guid.NewGuid():N}.csv");
        string[] args =
        [
            "scan",
            "--host", "192.168.1.50",
            "--unit-id", "1",
            "--table", "coils",
            "--start", "0",
            "--end", "0",
            "--format", "csv",
            "--output", outputPath
        ];
        ScriptedAddressProbeClient client = new(ModbusProbeOutcome.Readable(ScanValue.Bit(true)));
        using StringWriter output = new(CultureInfo.InvariantCulture);
        using StringWriter error = new(CultureInfo.InvariantCulture);

        try
        {
            int exitCode = await CliApplication.RunAsync(
                args,
                output,
                error,
                new SingleClientFactory(client));

            Assert.Equal(CliExitCodes.Success, exitCode);
            Assert.Contains("Scan completed. Results: 1.", output.ToString(), StringComparison.Ordinal);
            Assert.Equal(string.Empty, error.ToString());
            Assert.Contains("Table,Unit ID,Zero-based Address,Status,Value,Exception Code,Attempts,Duration ms,Message", File.ReadAllText(outputPath));
            Assert.Contains("coils,1,0,readable,true", File.ReadAllText(outputPath));
        }
        finally
        {
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
        }
    }

    [Fact]
    public async Task RunAsync_WithInvalidScanCommand_WritesErrorsAndReturnsInvalidOptions()
    {
        string[] args =
        [
            "scan",
            "--host", "plc.local",
            "--unit-id", "1",
            "--table", "holding-registers",
            "--start", "10",
            "--end", "9"
        ];

        using StringWriter output = new(CultureInfo.InvariantCulture);
        using StringWriter error = new(CultureInfo.InvariantCulture);

        int exitCode = await CliApplication.RunAsync(
            args,
            output,
            error,
            new SingleClientFactory(new ScriptedAddressProbeClient(ModbusProbeOutcome.Readable(ScanValue.Register(1)))));

        Assert.Equal(CliExitCodes.InvalidOptions, exitCode);
        Assert.Equal(string.Empty, output.ToString());
        Assert.Contains("Error: Option '--start' must be less than or equal to option '--end'.", error.ToString(), StringComparison.Ordinal);
        Assert.Contains("Usage:", error.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task RunAsync_WhenInitialConnectFails_ReturnsTargetUnreachable()
    {
        string[] args =
        [
            "scan",
            "--host", "plc.local",
            "--unit-id", "1",
            "--table", "holding-registers",
            "--start", "0",
            "--end", "0"
        ];
        ScriptedAddressProbeClient client = new(ModbusProbeOutcome.Readable(ScanValue.Register(1)))
        {
            ConnectException = new ModbusConnectionException("could not connect")
        };
        using StringWriter output = new(CultureInfo.InvariantCulture);
        using StringWriter error = new(CultureInfo.InvariantCulture);

        int exitCode = await CliApplication.RunAsync(args, output, error, new SingleClientFactory(client));

        Assert.Equal(CliExitCodes.TargetUnreachable, exitCode);
        Assert.Equal(string.Empty, output.ToString());
        Assert.Contains("Error: could not connect", error.ToString(), StringComparison.Ordinal);
    }
}
