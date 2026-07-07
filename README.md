# mitmi-scan

`mitmi-scan` is a read-only Modbus TCP address scanner for reverse engineering
undocumented devices.

The first version is intentionally narrow: it scans one Modbus address at a
time, records what can be read, and reports the observed register or bit map in
operator-friendly formats.

## Status

Initial CLI validation and report-model slices implemented.

The current executable parses `mitmi-scan scan ...`, validates the v0.1 command
contract, and prints a dry scan plan. It does not open network connections or
send Modbus requests yet.

The codebase also contains scan request/result models plus console, CSV, and
Markdown report renderers. Those renderers are ready for the upcoming scan loop
but are not yet wired to live Modbus traffic.

The Modbus TCP client adapter is implemented and simulator-tested, but the CLI
still stops at the dry scan plan until scan orchestration is wired.

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
- Report output as console summary, CSV, or Markdown table.
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
```

Example dry scan plan:

```powershell
dotnet run --project src\Mitmi.Scan.Cli -- scan --host 192.168.1.50 --unit-id 1 --table holding-registers --start 0 --end 9
```
