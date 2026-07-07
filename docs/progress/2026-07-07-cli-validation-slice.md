# 2026-07-07 CLI Validation Slice

## Scope

Implemented the first software-only slice for `mitmi-scan`.

This slice creates the .NET solution, console project, and test project. The CLI
accepts the planned `scan` command, validates options, and prints a dry scan
plan.

No Modbus library, TCP connection, simulator, or device interaction is included
in this slice.

## Implemented Behavior

- Command shape: `mitmi-scan scan ...`.
- Required options:
  - `--host`
  - `--unit-id`
  - `--table`
  - `--start`
  - `--end`
- Optional defaults:
  - `--port 502`
  - `--timeout-ms 1000`
  - `--delay-ms 10`
  - `--retries 0`
  - `--format console`
- Supported tables:
  - `coils`
  - `discrete-inputs`
  - `holding-registers`
  - `input-registers`
  - `all`
- Supported formats:
  - `console`
  - `csv`
  - `markdown`

CSV and Markdown currently require `--output`. Console output rejects
`--output` to avoid mixing progress/status and structured report behavior before
report rendering exists.

## Validation

Commands run:

```powershell
dotnet build Mitmi.Scan.slnx
dotnet test Mitmi.Scan.slnx
dotnet run --project src\Mitmi.Scan.Cli -- scan --host 192.168.1.50 --unit-id 1 --table all --start 0 --end 2 --format markdown --output scan.md
```

Result:

- Build succeeded with 0 warnings and 0 errors.
- Test suite passed: 19 tests.
- Manual dry-run output reported 12 planned probes for three addresses across
  all four tables.

## Next Recommended Slice

Implement scan result models and console, CSV, and Markdown renderers without
adding Modbus network traffic yet.
