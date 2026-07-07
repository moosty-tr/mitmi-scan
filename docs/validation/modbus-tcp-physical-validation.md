# Modbus TCP Physical Validation Runbook

## Purpose

Use this runbook before making field-ready claims for `mitmi-scan`.

Simulator tests prove scanner behavior against controlled Modbus TCP responses.
They do not prove that active scanning is safe for a real device, gateway, PLC,
or bench rig.

## Preconditions

- Device or bench rig is approved for active read-only probing.
- Operator understands that even read requests can affect weak or poorly
  implemented devices.
- Device network path is known.
- Target host, port, and unit ID are confirmed.
- Scan range is intentionally small for first contact.
- No production control process depends on the device during first validation.

## First Contact Command Shape

Start with one table and a small range:

```powershell
dotnet run --project src\Mitmi.Scan.Cli -- scan --host <target-ip> --unit-id <unit-id> --table holding-registers --start 0 --end 9 --timeout-ms 1000 --delay-ms 100 --retries 0 --format markdown --output docs\validation\results\<date>-physical-validation.md
```

Use:

- nonzero delay
- no retries
- explicit start and end addresses
- one table at a time

Do not begin with `--table all` on unknown equipment.

## Evidence To Preserve

Record:

- exact command line
- scanner commit
- device model
- firmware version when available
- network topology
- operator
- date and local time
- target host and port
- unit ID
- observed result file
- any device alarms, slowdowns, resets, or operator-visible effects

## Pass Criteria

- Scanner completes the requested range.
- Report contains one row per planned probe.
- Readable values and Modbus exceptions are recorded distinctly.
- Timeouts, transport errors, and malformed responses are recorded distinctly.
- Device remains stable during and after the scan.
- Operator confirms no unexpected side effects.

## Stop Criteria

Stop scanning if:

- device enters alarm or fault state
- scan causes visible process impact
- response latency grows unexpectedly
- repeated timeouts occur on a device expected to respond
- network path becomes unstable

## Expansion Path

After first contact succeeds:

1. Increase range within the same table.
2. Repeat for another single table.
3. Only then consider `--table all` for a modest range.
4. Preserve every command and report as validation evidence.
