namespace Mitmi.Scan.Tests;

internal sealed record TestModbusTcpRequest(
    ushort TransactionId,
    byte UnitId,
    byte FunctionCode,
    ushort Address,
    ushort Quantity);
