namespace Mitmi.Scan.Cli;

public static class CliExitCodes
{
    public const int Success = 0;
    public const int InvalidOptions = 1;
    public const int TargetUnreachable = 2;
    public const int RuntimeError = 3;
    public const int ReportOutputFailed = 4;
}
