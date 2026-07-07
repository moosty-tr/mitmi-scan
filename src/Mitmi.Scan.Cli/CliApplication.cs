using System.IO;

namespace Mitmi.Scan.Cli;

public static class CliApplication
{
    public static Task<int> RunAsync(string[] args, TextWriter output, TextWriter error)
    {
        return RunAsync(args, output, error, new NModbusTcpProbeClientFactory());
    }

    public static async Task<int> RunAsync(
        string[] args,
        TextWriter output,
        TextWriter error,
        IAddressProbeClientFactory clientFactory,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(error);
        ArgumentNullException.ThrowIfNull(clientFactory);

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

        ScanCommandOptions options = result.Options!;
        ScanRequest request = ScanRequest.FromOptions(options);
        ScanRunner runner = new(clientFactory);

        try
        {
            IReadOnlyList<ScanResult> scanResults = await runner.RunAsync(request, cancellationToken).ConfigureAwait(false);
            await ScanReportWriter.WriteAsync(options.Format, options.OutputPath, scanResults, output, cancellationToken).ConfigureAwait(false);
            return CliExitCodes.Success;
        }
        catch (ModbusConnectionException exception)
        {
            error.WriteLine($"Error: {exception.Message}");
            return CliExitCodes.TargetUnreachable;
        }
        catch (OperationCanceledException)
        {
            error.WriteLine("Error: Scan was canceled.");
            return CliExitCodes.RuntimeError;
        }
        catch (Exception exception) when (IsReportOutputException(exception))
        {
            error.WriteLine($"Error: Could not write report output: {exception.Message}");
            return CliExitCodes.ReportOutputFailed;
        }
        catch (Exception exception)
        {
            error.WriteLine($"Error: Scan failed: {exception.Message}");
            return CliExitCodes.RuntimeError;
        }
    }

    private static bool IsReportOutputException(Exception exception)
    {
        return exception is IOException
            || exception is UnauthorizedAccessException
            || exception is NotSupportedException;
    }
}
