using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Sockets;
using NModbus;

namespace Mitmi.Scan.Cli;

public sealed class NModbusTcpProbeClient : IAddressProbeClient
{
    private readonly string _host;
    private readonly int _port;
    private readonly int _timeoutMilliseconds;
    private readonly TimeSpan _timeout;
    private TcpClient? _tcpClient;
    private IModbusMaster? _master;

    public NModbusTcpProbeClient(string host, int port, int timeoutMilliseconds)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);
        ArgumentOutOfRangeException.ThrowIfLessThan(port, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(port, 65_535);
        ArgumentOutOfRangeException.ThrowIfLessThan(timeoutMilliseconds, 1);

        _host = host;
        _port = port;
        _timeoutMilliseconds = timeoutMilliseconds;
        _timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);
    }

    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        await DisposeConnectionAsync().ConfigureAwait(false);

        TcpClient tcpClient = new()
        {
            ReceiveTimeout = _timeoutMilliseconds,
            SendTimeout = _timeoutMilliseconds,
            NoDelay = true
        };

        try
        {
            await tcpClient.ConnectAsync(_host, _port, cancellationToken)
                .AsTask()
                .WaitAsync(_timeout, cancellationToken)
                .ConfigureAwait(false);

            ModbusFactory factory = new();
            IModbusMaster master = factory.CreateMaster(tcpClient);
            master.Transport.ReadTimeout = _timeoutMilliseconds;
            master.Transport.WriteTimeout = _timeoutMilliseconds;
            master.Transport.Retries = 0;
            master.Transport.SlaveBusyUsesRetryCount = true;

            _tcpClient = tcpClient;
            _master = master;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            tcpClient.Dispose();
            throw;
        }
        catch (Exception exception) when (IsTimeoutException(exception))
        {
            tcpClient.Dispose();
            throw new ModbusConnectionException($"Timed out connecting to {_host}:{_port}.", exception);
        }
        catch (Exception exception) when (IsTransportException(exception))
        {
            tcpClient.Dispose();
            throw new ModbusConnectionException($"Could not connect to {_host}:{_port}: {exception.Message}", exception);
        }
    }

    public async Task ReconnectAsync(CancellationToken cancellationToken)
    {
        await ConnectAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<ModbusProbeOutcome> ProbeAsync(ScanProbe probe, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(probe);

        IModbusMaster master = EnsureConnected();

        try
        {
            return await ReadOneAsync(master, probe)
                .WaitAsync(_timeout, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (SlaveException exception)
        {
            return ModbusProbeOutcome.ModbusException(
                checked((byte)exception.SlaveExceptionCode),
                exception.Message);
        }
        catch (Exception exception) when (IsTimeoutException(exception))
        {
            return ModbusProbeOutcome.Timeout(exception.Message);
        }
        catch (Exception exception) when (IsInvalidResponseException(exception))
        {
            return ModbusProbeOutcome.InvalidResponse(exception.Message);
        }
        catch (Exception exception) when (IsTransportException(exception))
        {
            return ModbusProbeOutcome.TransportError(exception.Message);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeConnectionAsync().ConfigureAwait(false);
    }

    private static async Task<ModbusProbeOutcome> ReadOneAsync(IModbusMaster master, ScanProbe probe)
    {
        ushort address = checked((ushort)probe.Address);

        return probe.Table switch
        {
            ModbusTable.Coils => BoolOutcome(await master.ReadCoilsAsync(probe.UnitId, address, numberOfPoints: 1).ConfigureAwait(false)),
            ModbusTable.DiscreteInputs => BoolOutcome(await master.ReadInputsAsync(probe.UnitId, address, numberOfPoints: 1).ConfigureAwait(false)),
            ModbusTable.HoldingRegisters => RegisterOutcome(await master.ReadHoldingRegistersAsync(probe.UnitId, address, numberOfPoints: 1).ConfigureAwait(false)),
            ModbusTable.InputRegisters => RegisterOutcome(await master.ReadInputRegistersAsync(probe.UnitId, address, numberOfPoints: 1).ConfigureAwait(false)),
            _ => throw new ArgumentOutOfRangeException(nameof(probe), probe.Table, "Unknown Modbus table.")
        };
    }

    private static ModbusProbeOutcome BoolOutcome(bool[] values)
    {
        if (values.Length != 1)
        {
            return ModbusProbeOutcome.InvalidResponse($"Expected 1 bit value but received {values.Length}.");
        }

        return ModbusProbeOutcome.Readable(ScanValue.Bit(values[0]));
    }

    private static ModbusProbeOutcome RegisterOutcome(ushort[] values)
    {
        if (values.Length != 1)
        {
            return ModbusProbeOutcome.InvalidResponse($"Expected 1 register value but received {values.Length}.");
        }

        return ModbusProbeOutcome.Readable(ScanValue.Register(values[0]));
    }

    private IModbusMaster EnsureConnected()
    {
        if (_master is null || _tcpClient is null || !_tcpClient.Connected)
        {
            throw new InvalidOperationException("The Modbus TCP client is not connected.");
        }

        return _master;
    }

    private async ValueTask DisposeConnectionAsync()
    {
        _master?.Dispose();
        _master = null;

        if (_tcpClient is not null)
        {
            _tcpClient.Dispose();
            _tcpClient = null;
        }
    }

    private static bool IsTimeoutException(Exception exception)
    {
        return exception is TimeoutException
            || exception.InnerException is TimeoutException
            || exception is IOException { InnerException: SocketException socketException } && socketException.SocketErrorCode == SocketError.TimedOut
            || exception is SocketException { SocketErrorCode: SocketError.TimedOut };
    }

    private static bool IsInvalidResponseException(Exception exception)
    {
        return exception is FormatException
            || exception is InvalidDataException
            || exception is ArgumentException;
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "The adapter maps library and socket failures into scanner outcomes.")]
    private static bool IsTransportException(Exception exception)
    {
        return exception is IOException
            || exception is SocketException
            || exception is ObjectDisposedException
            || exception is InvalidOperationException;
    }
}
