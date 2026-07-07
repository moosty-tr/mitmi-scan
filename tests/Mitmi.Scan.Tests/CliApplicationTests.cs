using System.Globalization;
using Mitmi.Scan.Cli;

namespace Mitmi.Scan.Tests;

public sealed class CliApplicationTests
{
    [Fact]
    public void Run_WithValidScanCommand_WritesDryPlanAndReturnsSuccess()
    {
        string[] args =
        [
            "scan",
            "--host", "192.168.1.50",
            "--unit-id", "1",
            "--table", "holding-registers",
            "--start", "0",
            "--end", "9"
        ];

        using StringWriter output = new(CultureInfo.InvariantCulture);
        using StringWriter error = new(CultureInfo.InvariantCulture);

        int exitCode = CliApplication.Run(args, output, error);

        Assert.Equal(CliExitCodes.Success, exitCode);
        Assert.Contains("Dry scan plan", output.ToString(), StringComparison.Ordinal);
        Assert.Contains("Target: 192.168.1.50:502", output.ToString(), StringComparison.Ordinal);
        Assert.Contains("Planned probes: 10", output.ToString(), StringComparison.Ordinal);
        Assert.Contains("Network: no connection will be opened", output.ToString(), StringComparison.Ordinal);
        Assert.Equal(string.Empty, error.ToString());
    }

    [Fact]
    public void Run_WithInvalidScanCommand_WritesErrorsAndReturnsInvalidOptions()
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

        int exitCode = CliApplication.Run(args, output, error);

        Assert.Equal(CliExitCodes.InvalidOptions, exitCode);
        Assert.Equal(string.Empty, output.ToString());
        Assert.Contains("Error: Option '--start' must be less than or equal to option '--end'.", error.ToString(), StringComparison.Ordinal);
        Assert.Contains("Usage:", error.ToString(), StringComparison.Ordinal);
    }
}
