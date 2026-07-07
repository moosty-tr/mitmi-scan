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
}
