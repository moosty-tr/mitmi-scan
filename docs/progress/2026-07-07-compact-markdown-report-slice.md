# 2026-07-07 Compact Markdown Report Slice

## Trigger

Physical-device validation showed that the previous Markdown report was too
verbose for the primary workflow. It listed failed reads with long exception
messages, which made the available address map harder to read.

## Decision

Keep the durable scan model and CSV output exhaustive, but make Markdown a
compact operator-facing discovery report.

Markdown now:

- lists successful reads only
- omits failed reads and exception messages
- omits per-row duration
- shows values as:
  - hex
  - decimal
  - ASCII
  - binary
- ends with a short scan summary sentence

CSV remains the better format for exhaustive evidence and failure analysis.

## Validation

Commands run:

```powershell
dotnet build Mitmi.Scan.slnx
dotnet test Mitmi.Scan.slnx --no-build
```

Result:

- Build succeeded with 0 warnings and 0 errors.
- Test suite passed: 48 tests.

## Note

The physical validation result file created during field testing was left
unstaged. It is useful evidence, but it should only be committed after an
intentional review for device details or other sensitive context.
