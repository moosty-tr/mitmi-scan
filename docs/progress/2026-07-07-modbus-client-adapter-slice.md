# 2026-07-07 Modbus Client Adapter Slice

## Scope

Implemented the Modbus TCP client adapter behind a small scanner-owned
interface.

This slice introduces NModbus for production Modbus TCP reads. The custom
protocol code added in this slice is limited to a deterministic test simulator.

## Implemented Behavior

- Added `IAddressProbeClient` for one-attempt address probes.
- Added `NModbusTcpProbeClient` for Modbus TCP reads.
- Added connection and reconnection methods for future scan-loop retry handling.
- Configured the NModbus transport with:
  - per-request read timeout
  - per-request write timeout
  - no library-level retries
  - bounded slave-busy retry behavior
- Mapped adapter outcomes to scanner statuses:
  - readable
  - modbus-exception
  - timeout
  - transport-error
  - invalid-response
- Kept retry count, elapsed duration, and final `ScanResult` construction out
  of the adapter. Those belong to scan orchestration.

## Validation

Commands run:

```powershell
dotnet build Mitmi.Scan.slnx
dotnet test Mitmi.Scan.slnx --no-build
```

Result:

- Build succeeded with 0 warnings and 0 errors.
- Test suite passed: 35 tests.

Simulator-backed tests covered:

- readable holding register
- readable coil
- Modbus exception response
- request timeout
- malformed response
- closed-port connection failure

## Next Recommended Slice

Implement scan orchestration:

- build concrete probes from `ScanRequest`
- scan in deterministic table/address order
- apply retry policy only to timeout and transport-error outcomes
- reconnect before retrying or continuing after uncertain connection outcomes
- render reports from actual scan results
