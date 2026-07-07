using Mitmi.Scan.Cli;

namespace Mitmi.Scan.Tests;

internal sealed class ScriptedAddressProbeClient : IAddressProbeClient
{
    private readonly Queue<ModbusProbeOutcome> _outcomes;
    private readonly ModbusProbeOutcome _fallbackOutcome;

    public ScriptedAddressProbeClient(params ModbusProbeOutcome[] outcomes)
    {
        if (outcomes.Length == 0)
        {
            throw new ArgumentException("At least one outcome is required.", nameof(outcomes));
        }

        _outcomes = new Queue<ModbusProbeOutcome>(outcomes);
        _fallbackOutcome = outcomes[^1];
    }

    public List<ScanProbe> Probes { get; } = [];

    public int ConnectCount { get; private set; }

    public int ReconnectCount { get; private set; }

    public ModbusConnectionException? ConnectException { get; init; }

    public Task ConnectAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ConnectCount++;

        if (ConnectException is not null)
        {
            throw ConnectException;
        }

        return Task.CompletedTask;
    }

    public Task ReconnectAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ReconnectCount++;
        return Task.CompletedTask;
    }

    public Task<ModbusProbeOutcome> ProbeAsync(ScanProbe probe, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Probes.Add(probe);

        if (_outcomes.Count > 0)
        {
            return Task.FromResult(_outcomes.Dequeue());
        }

        return Task.FromResult(_fallbackOutcome);
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}
