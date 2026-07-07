# mitmi-scan

`mitmi-scan` is a read-only Modbus TCP address scanner for reverse engineering
undocumented devices.

The first version is intentionally narrow: it scans one Modbus address at a
time, records what can be read, and reports the observed register or bit map in
operator-friendly formats.

## Status

Single-table/all-table scan orchestration with progress reporting implemented.

The current executable parses `mitmi-scan scan ...`, validates the v0.1 command
contract, opens a Modbus TCP connection, sends one read-only request per
selected table/address pair, and renders console, CSV, or Markdown reports.

Markdown reports list successful reads only and show values as hex, decimal,
ASCII, and binary with a short scan summary. CSV remains the exhaustive
machine-readable output when failure details are needed.

The scanner is simulator-tested. Field-readiness still requires validation
against an approved Modbus TCP device or bench rig.

## Intended v0.1 Scope

- Modbus TCP.
- Single-address scanning only.
- Read-only functions:
  - coils
  - discrete inputs
  - holding registers
  - input registers
- Configurable target host, port, unit ID, address range, timeout, delay, and
  retry count.
- Console progress with estimated time remaining and an ASCII progress bar.
- Report output as console summary, exhaustive CSV, or compact Markdown table.
- Zero-based PDU addresses as the canonical address representation.

## Out Of Scope For v0.1

- Write functions.
- Adaptive range scanning.
- Response caching.
- Proxy behavior.
- Multi-client mediation.
- MQTT, OPC UA, HTTP, or other protocol conversion.
- Passive packet capture.
- Automatic device-type identification.

## Design Notes

Start here before implementing:

- [Vision](VISION.md)
- [Scanner architecture](docs/architecture/modbus-address-scanner-design.md)
- [v0.1 implementation plan](docs/planning/v0.1-implementation-plan.md)
- [Fresh-session handoff](docs/progress/2026-07-07-fresh-session-handoff.md)

Decision records:

- [ADR-0001: Keep mitmi-scan separate from MITMI](docs/decisions/ADR-0001-separate-scanner-tool.md)
- [ADR-0002: Use read-only single-address scanning for v0.1](docs/decisions/ADR-0002-single-address-read-only-scan.md)
- [ADR-0003: Use zero-based addresses as the canonical report key](docs/decisions/ADR-0003-addressing-and-reporting.md)

## Development

Build and test:

```powershell
dotnet build Mitmi.Scan.slnx
dotnet test Mitmi.Scan.slnx
.\scripts\Invoke-ReleaseSmokeTest.ps1
```

Publish a Windows executable:

```powershell
.\scripts\Publish-Release.ps1
.\artifacts\publish\mitmi-scan-win-x64\mitmi-scan.exe --help
```

Example scan:

```powershell
dotnet run --project src\Mitmi.Scan.Cli -- scan --host 192.168.1.50 --unit-id 1 --table holding-registers --start 0 --end 9
```

Compact Markdown output:

```markdown
| Table | Address | Hex | Decimal | ASCII | Binary |
| --- | --- | --- | --- | --- | --- |
| holding-registers | 0 | 0x4142 | 16706 | AB | 0b0100000101000010 |

Scanned 10 holding registers in 1.234 seconds; found total of 1 active register.
```
