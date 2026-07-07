using Mitmi.Scan.Cli;

namespace Mitmi.Scan.Tests;

internal sealed class SingleClientFactory : IAddressProbeClientFactory
{
    private readonly IAddressProbeClient _client;

    public SingleClientFactory(IAddressProbeClient client)
    {
        _client = client;
    }

    public IAddressProbeClient Create(ScanRequest request)
    {
        _ = request;
        return _client;
    }
}
