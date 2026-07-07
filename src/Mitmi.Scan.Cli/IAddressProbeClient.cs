namespace Mitmi.Scan.Cli;

public interface IAddressProbeClient : IAsyncDisposable
{
    Task ConnectAsync(CancellationToken cancellationToken);

    Task ReconnectAsync(CancellationToken cancellationToken);

    Task<ModbusProbeOutcome> ProbeAsync(ScanProbe probe, CancellationToken cancellationToken);
}
