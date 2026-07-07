using System.Globalization;

namespace Mitmi.Scan.Cli;

public sealed record ScanValue
{
    private ScanValue(ScanValueKind kind, bool? bitValue, ushort? registerValue)
    {
        Kind = kind;
        BitValue = bitValue;
        RegisterValue = registerValue;
    }

    public ScanValueKind Kind { get; }

    public bool? BitValue { get; }

    public ushort? RegisterValue { get; }

    public static ScanValue Bit(bool value)
    {
        return new ScanValue(ScanValueKind.Bit, value, registerValue: null);
    }

    public static ScanValue Register(ushort value)
    {
        return new ScanValue(ScanValueKind.Register, bitValue: null, value);
    }

    public string ToReportValue()
    {
        return Kind switch
        {
            ScanValueKind.Bit => BitValue!.Value ? "true" : "false",
            ScanValueKind.Register => RegisterValue!.Value.ToString(CultureInfo.InvariantCulture),
            _ => throw new ArgumentOutOfRangeException(nameof(Kind), Kind, "Unknown scan value kind.")
        };
    }

    public string ToHexReportValue()
    {
        return Kind switch
        {
            ScanValueKind.Bit => BitValue!.Value ? "0x01" : "0x00",
            ScanValueKind.Register => $"0x{RegisterValue!.Value.ToString("X4", CultureInfo.InvariantCulture)}",
            _ => throw new ArgumentOutOfRangeException(nameof(Kind), Kind, "Unknown scan value kind.")
        };
    }

    public string ToDecimalReportValue()
    {
        return Kind switch
        {
            ScanValueKind.Bit => BitValue!.Value ? "1" : "0",
            ScanValueKind.Register => RegisterValue!.Value.ToString(CultureInfo.InvariantCulture),
            _ => throw new ArgumentOutOfRangeException(nameof(Kind), Kind, "Unknown scan value kind.")
        };
    }

    public string ToAsciiReportValue()
    {
        return Kind switch
        {
            ScanValueKind.Bit => string.Empty,
            ScanValueKind.Register => FormatRegisterAscii(RegisterValue!.Value),
            _ => throw new ArgumentOutOfRangeException(nameof(Kind), Kind, "Unknown scan value kind.")
        };
    }

    public string ToBinaryReportValue()
    {
        return Kind switch
        {
            ScanValueKind.Bit => BitValue!.Value ? "1" : "0",
            ScanValueKind.Register => $"0b{Convert.ToString(RegisterValue!.Value, 2).PadLeft(16, '0')}",
            _ => throw new ArgumentOutOfRangeException(nameof(Kind), Kind, "Unknown scan value kind.")
        };
    }

    private static string FormatRegisterAscii(ushort value)
    {
        char high = FormatAsciiByte((byte)(value >> 8));
        char low = FormatAsciiByte((byte)value);
        return new string([high, low]);
    }

    private static char FormatAsciiByte(byte value)
    {
        return value is >= 0x20 and <= 0x7E ? (char)value : '.';
    }
}
