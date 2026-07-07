# 2026-07-07 Scan Orchestration Slice

## Scope

Implemented scan orchestration and wired the CLI to execute real Modbus TCP
scans.

This slice moves the executable beyond dry scan plans.

## Implemented Behavior

- Builds concrete probes from `ScanRequest`.
- Scans in deterministic table-then-address order.
- Uses one read-only request per selected table/address pair.
- Applies retry policy only to:
  - timeout
  - transport-error
- Does not retry Modbus exception responses.
- Reconnects before retrying after timeout or transport-error outcomes.
- Records result duration, timestamp, attempt count, status, value, exception
  code, and message.
- Writes console reports to stdout.
- Writes CSV and Markdown reports to the requested output file.
- Uses stable process exit codes:
  - `0`: success
  - `1`: invalid options
  - `2`: target unreachable before scanning starts
  - `3`: canceled or unrecoverable runtime error
  - `4`: report output failure

## Validation

Commands run:

```powershell
dotnet build Mitmi.Scan.slnx
dotnet test Mitmi.Scan.slnx --no-build
```

Result:

- Build succeeded with 0 warnings and 0 errors.
- Test suite passed: 41 tests.

Tests covered:

- CLI execution with fake probe client.
- CSV output file creation.
- invalid option handling.
- initial connection failure exit code.
- deterministic scan order.
- retry on timeout.
- no retry on Modbus exception.
- retry-budget exhaustion for transport errors.
- simulator-backed Modbus TCP adapter behavior from the previous slice.

## Next Recommended Slice

Add progress reporting and ETA:

- progress sink abstraction
- interactive ASCII progress bar
- line-oriented progress for redirected output
- tests for progress calculation and rendering
