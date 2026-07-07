namespace Mitmi.Scan.Tests;

internal sealed record TestModbusTcpResponse(byte[]? Pdu, bool KeepConnectionOpen)
{
    public static TestModbusTcpResponse Bit(TestModbusTcpRequest request, bool value)
    {
        return new TestModbusTcpResponse([request.FunctionCode, 0x01, value ? (byte)0x01 : (byte)0x00], KeepConnectionOpen: false);
    }

    public static TestModbusTcpResponse Register(TestModbusTcpRequest request, ushort value)
    {
        return new TestModbusTcpResponse([request.FunctionCode, 0x02, (byte)(value >> 8), (byte)value], KeepConnectionOpen: false);
    }

    public static TestModbusTcpResponse ModbusException(TestModbusTcpRequest request, byte exceptionCode)
    {
        return new TestModbusTcpResponse([(byte)(request.FunctionCode | 0x80), exceptionCode], KeepConnectionOpen: false);
    }

    public static TestModbusTcpResponse RawPdu(TestModbusTcpRequest request, byte[] pdu)
    {
        _ = request;
        return new TestModbusTcpResponse(pdu, KeepConnectionOpen: false);
    }

    public static TestModbusTcpResponse NoResponse()
    {
        return new TestModbusTcpResponse(Pdu: null, KeepConnectionOpen: true);
    }
}
