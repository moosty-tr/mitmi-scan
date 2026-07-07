namespace Mitmi.Scan.Cli;

public static class CliUsage
{
    public const string Text = """
Usage:
  mitmi-scan scan --host <host> --unit-id <0-255> --table <table> --start <0-65535> --end <0-65535> [options]

Required options:
  --host <host>          Target Modbus TCP host or IP address.
  --unit-id <0-255>     Modbus TCP unit identifier.
  --table <table>       coils, discrete-inputs, holding-registers, input-registers, or all.
  --start <address>     Zero-based inclusive start address.
  --end <address>       Zero-based inclusive end address.

Optional options:
  --port <1-65535>      Target TCP port. Default: 502.
  --timeout-ms <ms>     Per-request timeout. Default: 1000.
  --delay-ms <ms>       Delay between probes. Default: 10.
  --retries <count>     Retries for timeout or transport errors. Default: 0.
  --format <format>     console, csv, or markdown. Default: console.
  --output <path>       Required for csv and markdown reports.

This implementation slice prints a dry scan plan only. It does not open network connections.
""";
}
