using System.Diagnostics;

namespace Mitmi.Scan.Cli;

public sealed class ScanRunner
{
    private readonly IAddressProbeClientFactory _clientFactory;

    public ScanRunner(IAddressProbeClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task<IReadOnlyList<ScanResult>> RunAsync(ScanRequest request, CancellationToken cancellationToken)
    {
        return await RunAsync(request, NoOpScanProgressSink.Instance, cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<ScanResult>> RunAsync(
        ScanRequest request,
        IScanProgressSink progressSink,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(progressSink);

        List<ScanResult> results = new(capacity: checked((int)Math.Min(request.PlannedProbeCount, int.MaxValue)));
        Stopwatch scanStopwatch = Stopwatch.StartNew();
        long completedProbes = 0;

        await using IAddressProbeClient client = _clientFactory.Create(request);
        await client.ConnectAsync(cancellationToken).ConfigureAwait(false);
        progressSink.ScanStarted(request);

        foreach (ScanProbe probe in BuildProbes(request))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ScanResult result = await ProbeWithRetryAsync(client, request, probe, cancellationToken).ConfigureAwait(false);
            results.Add(result);
            completedProbes++;
            progressSink.ProbeCompleted(new ScanProgressSnapshot(completedProbes, request.PlannedProbeCount, scanStopwatch.Elapsed));

            if (request.DelayMilliseconds > 0)
            {
                await Task.Delay(request.DelayMilliseconds, cancellationToken).ConfigureAwait(false);
            }
        }

        scanStopwatch.Stop();
        progressSink.ScanCompleted(new ScanProgressSnapshot(completedProbes, request.PlannedProbeCount, scanStopwatch.Elapsed));
        return results;
    }

    private static IEnumerable<ScanProbe> BuildProbes(ScanRequest request)
    {
        foreach (ModbusTable table in request.Tables)
        {
            for (int address = request.StartAddress; address <= request.EndAddress; address++)
            {
                yield return new ScanProbe(request.Host, request.Port, request.UnitId, table, address);
            }
        }
    }

    private static async Task<ScanResult> ProbeWithRetryAsync(
        IAddressProbeClient client,
        ScanRequest request,
        ScanProbe probe,
        CancellationToken cancellationToken)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        int maxAttempts = request.Retries + 1;
        int attempts = 0;
        ModbusProbeOutcome? finalOutcome = null;

        while (attempts < maxAttempts)
        {
            attempts++;
            finalOutcome = await client.ProbeAsync(probe, cancellationToken).ConfigureAwait(false);

            if (!ShouldRetry(finalOutcome.Status) || attempts >= maxAttempts)
            {
                break;
            }

            string? reconnectFailure = await TryReconnectAsync(client, cancellationToken).ConfigureAwait(false);
            if (reconnectFailure is not null)
            {
                finalOutcome = ModbusProbeOutcome.TransportError(reconnectFailure);
                break;
            }
        }

        stopwatch.Stop();

        if (finalOutcome is null)
        {
            finalOutcome = ModbusProbeOutcome.TransportError("No probe attempt was completed.");
        }

        if (ShouldRetry(finalOutcome.Status))
        {
            _ = await TryReconnectAsync(client, cancellationToken).ConfigureAwait(false);
        }

        return ToScanResult(probe, finalOutcome, attempts, stopwatch.Elapsed, DateTimeOffset.UtcNow);
    }

    private static bool ShouldRetry(ScanResultStatus status)
    {
        return status is ScanResultStatus.Timeout or ScanResultStatus.TransportError;
    }

    private static async Task<string?> TryReconnectAsync(IAddressProbeClient client, CancellationToken cancellationToken)
    {
        try
        {
            await client.ReconnectAsync(cancellationToken).ConfigureAwait(false);
            return null;
        }
        catch (ModbusConnectionException exception)
        {
            return $"Reconnect failed: {exception.Message}";
        }
    }

    private static ScanResult ToScanResult(
        ScanProbe probe,
        ModbusProbeOutcome outcome,
        int attempts,
        TimeSpan duration,
        DateTimeOffset timestampUtc)
    {
        return outcome.Status switch
        {
            ScanResultStatus.Readable => ScanResult.Readable(probe, outcome.Value!, attempts, duration, timestampUtc, outcome.Message),
            ScanResultStatus.ModbusException => ScanResult.ModbusException(probe, outcome.ModbusExceptionCode ?? 0, attempts, duration, timestampUtc, outcome.Message),
            ScanResultStatus.Timeout => ScanResult.Timeout(probe, attempts, duration, timestampUtc, outcome.Message),
            ScanResultStatus.TransportError => ScanResult.TransportError(probe, attempts, duration, timestampUtc, outcome.Message),
            ScanResultStatus.InvalidResponse => ScanResult.InvalidResponse(probe, attempts, duration, timestampUtc, outcome.Message),
            _ => throw new ArgumentOutOfRangeException(nameof(outcome), outcome.Status, "Unknown scan outcome.")
        };
    }
}
