namespace Mitmi.Scan.Cli;

public sealed class NModbusTcpProbeClientFactory : IAddressProbeClientFactory
{
    public IAddressProbeClient Create(ScanRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new NModbusTcpProbeClient(request.Host, request.Port, request.TimeoutMilliseconds);
    }
}
