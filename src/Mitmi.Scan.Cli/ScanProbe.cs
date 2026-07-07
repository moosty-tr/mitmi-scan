namespace Mitmi.Scan.Cli;

public sealed record ScanProbe
{
    public ScanProbe(string host, int port, byte unitId, ModbusTable table, int address)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);
        ArgumentOutOfRangeException.ThrowIfLessThan(port, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(port, 65_535);
        ArgumentOutOfRangeException.ThrowIfLessThan(address, 0);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(address, 65_535);

        Host = host;
        Port = port;
        UnitId = unitId;
        Table = table;
        Address = address;
    }

    public string Host { get; }

    public int Port { get; }

    public byte UnitId { get; }

    public ModbusTable Table { get; }

    public int Address { get; }
}
