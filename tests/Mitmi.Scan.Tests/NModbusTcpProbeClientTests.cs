using Mitmi.Scan.Cli;

namespace Mitmi.Scan.Tests;

public sealed class NModbusTcpProbeClientTests
{
    [Fact]
    public async Task ProbeAsync_ReadHoldingRegister_ReturnsReadableRegister()
    {
        await using TestModbusTcpServer server = TestModbusTcpServer.Start(request =>
            TestModbusTcpResponse.Register(request, 0x1234));
        await using NModbusTcpProbeClient client = new("127.0.0.1", server.Port, timeoutMilliseconds: 1_000);

        await client.ConnectAsync(CancellationToken.None);
        ModbusProbeOutcome outcome = await client.ProbeAsync(
            new ScanProbe("127.0.0.1", server.Port, 1, ModbusTable.HoldingRegisters, 10),
            CancellationToken.None);

        Assert.Equal(ScanResultStatus.Readable, outcome.Status);
        Assert.Equal("4660", outcome.Value?.ToReportValue());
        TestModbusTcpRequest request = Assert.Single(server.Requests);
        Assert.Equal(0x03, request.FunctionCode);
        Assert.Equal(10, request.Address);
        Assert.Equal(1, request.Quantity);
    }

    [Fact]
    public async Task ProbeAsync_ReadCoil_ReturnsReadableBit()
    {
        await using TestModbusTcpServer server = TestModbusTcpServer.Start(request =>
            TestModbusTcpResponse.Bit(request, true));
        await using NModbusTcpProbeClient client = new("127.0.0.1", server.Port, timeoutMilliseconds: 1_000);

        await client.ConnectAsync(CancellationToken.None);
        ModbusProbeOutcome outcome = await client.ProbeAsync(
            new ScanProbe("127.0.0.1", server.Port, 1, ModbusTable.Coils, 5),
            CancellationToken.None);

        Assert.Equal(ScanResultStatus.Readable, outcome.Status);
        Assert.Equal("true", outcome.Value?.ToReportValue());
        TestModbusTcpRequest request = Assert.Single(server.Requests);
        Assert.Equal(0x01, request.FunctionCode);
        Assert.Equal(5, request.Address);
        Assert.Equal(1, request.Quantity);
    }

    [Fact]
    public async Task ProbeAsync_ModbusExceptionResponse_ReturnsModbusExceptionOutcome()
    {
        await using TestModbusTcpServer server = TestModbusTcpServer.Start(request =>
            TestModbusTcpResponse.ModbusException(request, exceptionCode: 0x02));
        await using NModbusTcpProbeClient client = new("127.0.0.1", server.Port, timeoutMilliseconds: 1_000);

        await client.ConnectAsync(CancellationToken.None);
        ModbusProbeOutcome outcome = await client.ProbeAsync(
            new ScanProbe("127.0.0.1", server.Port, 1, ModbusTable.InputRegisters, 999),
            CancellationToken.None);

        Assert.Equal(ScanResultStatus.ModbusException, outcome.Status);
        Assert.Equal((byte)0x02, outcome.ModbusExceptionCode);
    }

    [Fact]
    public async Task ProbeAsync_WhenServerDoesNotRespond_ReturnsTimeoutOutcome()
    {
        await using TestModbusTcpServer server = TestModbusTcpServer.Start(request =>
            TestModbusTcpResponse.NoResponse());
        await using NModbusTcpProbeClient client = new("127.0.0.1", server.Port, timeoutMilliseconds: 100);

        await client.ConnectAsync(CancellationToken.None);
        ModbusProbeOutcome outcome = await client.ProbeAsync(
            new ScanProbe("127.0.0.1", server.Port, 1, ModbusTable.HoldingRegisters, 1),
            CancellationToken.None);

        Assert.Equal(ScanResultStatus.Timeout, outcome.Status);
    }

    [Fact]
    public async Task ProbeAsync_WhenServerReturnsMalformedPayload_ReturnsInvalidResponseOutcome()
    {
        await using TestModbusTcpServer server = TestModbusTcpServer.Start(request =>
            TestModbusTcpResponse.RawPdu(request, [request.FunctionCode, 0x02, 0x12]));
        await using NModbusTcpProbeClient client = new("127.0.0.1", server.Port, timeoutMilliseconds: 1_000);

        await client.ConnectAsync(CancellationToken.None);
        ModbusProbeOutcome outcome = await client.ProbeAsync(
            new ScanProbe("127.0.0.1", server.Port, 1, ModbusTable.HoldingRegisters, 1),
            CancellationToken.None);

        Assert.Equal(ScanResultStatus.InvalidResponse, outcome.Status);
    }

    [Fact]
    public async Task ConnectAsync_WhenPortIsClosed_ThrowsConnectionException()
    {
        int unusedPort = GetCurrentlyUnusedLoopbackPort();
        await using NModbusTcpProbeClient client = new("127.0.0.1", unusedPort, timeoutMilliseconds: 250);

        await Assert.ThrowsAsync<ModbusConnectionException>(() =>
            client.ConnectAsync(CancellationToken.None));
    }

    private static int GetCurrentlyUnusedLoopbackPort()
    {
        TcpListenerProbe listener = TcpListenerProbe.Start();
        int port = listener.Port;
        listener.Dispose();
        return port;
    }
}
