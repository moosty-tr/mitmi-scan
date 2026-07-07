namespace Mitmi.Scan.Cli;

public enum ScanTableSelection
{
    Coils,
    DiscreteInputs,
    HoldingRegisters,
    InputRegisters,
    All
}

public static class ScanTableSelectionExtensions
{
    public static bool TryParseCliName(string value, out ScanTableSelection table)
    {
        switch (value)
        {
            case "coils":
                table = ScanTableSelection.Coils;
                return true;
            case "discrete-inputs":
                table = ScanTableSelection.DiscreteInputs;
                return true;
            case "holding-registers":
                table = ScanTableSelection.HoldingRegisters;
                return true;
            case "input-registers":
                table = ScanTableSelection.InputRegisters;
                return true;
            case "all":
                table = ScanTableSelection.All;
                return true;
            default:
                table = default;
                return false;
        }
    }

    public static string ToCliName(this ScanTableSelection table)
    {
        return table switch
        {
            ScanTableSelection.Coils => "coils",
            ScanTableSelection.DiscreteInputs => "discrete-inputs",
            ScanTableSelection.HoldingRegisters => "holding-registers",
            ScanTableSelection.InputRegisters => "input-registers",
            ScanTableSelection.All => "all",
            _ => throw new ArgumentOutOfRangeException(nameof(table), table, "Unknown scan table.")
        };
    }

    public static int TableCount(this ScanTableSelection table)
    {
        return table == ScanTableSelection.All ? 4 : 1;
    }

    public static IReadOnlyList<ModbusTable> ExpandTables(this ScanTableSelection table)
    {
        return table switch
        {
            ScanTableSelection.Coils => [ModbusTable.Coils],
            ScanTableSelection.DiscreteInputs => [ModbusTable.DiscreteInputs],
            ScanTableSelection.HoldingRegisters => [ModbusTable.HoldingRegisters],
            ScanTableSelection.InputRegisters => [ModbusTable.InputRegisters],
            ScanTableSelection.All =>
            [
                ModbusTable.Coils,
                ModbusTable.DiscreteInputs,
                ModbusTable.HoldingRegisters,
                ModbusTable.InputRegisters
            ],
            _ => throw new ArgumentOutOfRangeException(nameof(table), table, "Unknown scan table.")
        };
    }

    public static string ToPlanDisplayName(this ScanTableSelection table)
    {
        if (table != ScanTableSelection.All)
        {
            return table.ToCliName();
        }

        return "all (coils, discrete-inputs, holding-registers, input-registers)";
    }
}
