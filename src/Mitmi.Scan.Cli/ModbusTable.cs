namespace Mitmi.Scan.Cli;

public enum ModbusTable
{
    Coils,
    DiscreteInputs,
    HoldingRegisters,
    InputRegisters
}

public static class ModbusTableExtensions
{
    public static string ToCliName(this ModbusTable table)
    {
        return table switch
        {
            ModbusTable.Coils => "coils",
            ModbusTable.DiscreteInputs => "discrete-inputs",
            ModbusTable.HoldingRegisters => "holding-registers",
            ModbusTable.InputRegisters => "input-registers",
            _ => throw new ArgumentOutOfRangeException(nameof(table), table, "Unknown Modbus table.")
        };
    }
}
