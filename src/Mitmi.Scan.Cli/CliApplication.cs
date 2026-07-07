namespace Mitmi.Scan.Cli;

public static class CliApplication
{
    public static int Run(string[] args, TextWriter output, TextWriter error)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(error);

        CommandParseResult result = ScanCommandParser.Parse(args);

        if (result.HelpRequested)
        {
            output.WriteLine(CliUsage.Text);
            return CliExitCodes.Success;
        }

        if (!result.Succeeded)
        {
            foreach (string parseError in result.Errors)
            {
                error.WriteLine($"Error: {parseError}");
            }

            error.WriteLine();
            error.WriteLine(CliUsage.Text);
            return CliExitCodes.InvalidOptions;
        }

        ScanPlanRenderer.Write(result.Options!, output);
        return CliExitCodes.Success;
    }
}
