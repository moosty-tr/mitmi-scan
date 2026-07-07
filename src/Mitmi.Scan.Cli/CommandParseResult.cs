namespace Mitmi.Scan.Cli;

public sealed record CommandParseResult(
    ScanCommandOptions? Options,
    IReadOnlyList<string> Errors,
    bool HelpRequested)
{
    public bool Succeeded => Options is not null && Errors.Count == 0 && !HelpRequested;

    public static CommandParseResult Success(ScanCommandOptions options)
    {
        return new CommandParseResult(options, Array.Empty<string>(), HelpRequested: false);
    }

    public static CommandParseResult Invalid(IReadOnlyList<string> errors)
    {
        return new CommandParseResult(null, errors, HelpRequested: false);
    }

    public static CommandParseResult Help()
    {
        return new CommandParseResult(null, Array.Empty<string>(), HelpRequested: true);
    }
}
