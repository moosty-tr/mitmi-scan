namespace Mitmi.Scan.Cli;

public interface IAddressProbeClientFactory
{
    IAddressProbeClient Create(ScanRequest request);
}
