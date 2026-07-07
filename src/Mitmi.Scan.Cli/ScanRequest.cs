namespace Mitmi.Scan.Cli;

public sealed record ScanRequest
{
    public ScanRequest(
        string host,
        int port,
        byte unitId,
        IReadOnlyList<ModbusTable> tables,
        int startAddress,
        int endAddress,
        int timeoutMilliseconds,
        int delayMilliseconds,
        int retries)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);
        ArgumentOutOfRangeException.ThrowIfLessThan(port, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(port, 65_535);
        ArgumentNullException.ThrowIfNull(tables);

        if (tables.Count == 0)
        {
            throw new ArgumentException("At least one table must be selected.", nameof(tables));
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(startAddress, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(startAddress, 65_535);
        ArgumentOutOfRangeException.ThrowIfLessThan(endAddress, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(endAddress, 65_535);

        if (startAddress > endAddress)
        {
            throw new ArgumentException("Start address must be less than or equal to end address.", nameof(startAddress));
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(timeoutMilliseconds, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(delayMilliseconds, 0);
        ArgumentOutOfRangeException.ThrowIfLessThan(retries, 0);

        Host = host;
        Port = port;
        UnitId = unitId;
        Tables = tables;
        StartAddress = startAddress;
        EndAddress = endAddress;
        TimeoutMilliseconds = timeoutMilliseconds;
        DelayMilliseconds = delayMilliseconds;
        Retries = retries;
    }

    public string Host { get; }

    public int Port { get; }

    public byte UnitId { get; }

    public IReadOnlyList<ModbusTable> Tables { get; }

    public int StartAddress { get; }

    public int EndAddress { get; }

    public int TimeoutMilliseconds { get; }

    public int DelayMilliseconds { get; }

    public int Retries { get; }

    public int AddressCount => EndAddress - StartAddress + 1;

    public long PlannedProbeCount => (long)AddressCount * Tables.Count;

    public static ScanRequest FromOptions(ScanCommandOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return new ScanRequest(
            options.Host,
            options.Port,
            options.UnitId,
            options.Table.ExpandTables(),
            options.StartAddress,
            options.EndAddress,
            options.TimeoutMilliseconds,
            options.DelayMilliseconds,
            options.Retries);
    }
}
