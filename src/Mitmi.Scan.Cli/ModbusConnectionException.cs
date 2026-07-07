namespace Mitmi.Scan.Cli;

public sealed class ModbusConnectionException : Exception
{
    public ModbusConnectionException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
