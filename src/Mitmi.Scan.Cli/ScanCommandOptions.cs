namespace Mitmi.Scan.Cli;

public sealed record ScanCommandOptions(
    string Host,
    int Port,
    byte UnitId,
    ScanTableSelection Table,
    int StartAddress,
    int EndAddress,
    int TimeoutMilliseconds,
    int DelayMilliseconds,
    int Retries,
    ReportFormat Format,
    string? OutputPath)
{
    public const int DefaultPort = 502;
    public const int DefaultTimeoutMilliseconds = 1_000;
    public const int DefaultDelayMilliseconds = 10;
    public const int DefaultRetries = 0;

    public int AddressCount => EndAddress - StartAddress + 1;

    public long PlannedProbeCount => (long)AddressCount * Table.TableCount();
}
