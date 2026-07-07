# 2026-07-07 Simulator Validation

## Scope

Simulator-backed validation covers the current software behavior for
`mitmi-scan`.

No physical device, gateway, PLC, or bench rig was used.

## Commands

```powershell
dotnet build Mitmi.Scan.slnx
dotnet test Mitmi.Scan.slnx --no-build
```

Release smoke command:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\Invoke-ReleaseSmokeTest.ps1
```

## Result

- Debug build succeeded with 0 warnings and 0 errors.
- Debug test suite passed: 46 tests.
- Release smoke succeeded.
- Release build succeeded with 0 warnings and 0 errors.
- Release test suite passed: 46 tests.
- Release CLI help command completed successfully.

Simulator tests cover:

- readable holding register
- readable coil
- Modbus exception response
- timeout
- malformed response
- closed-port connection failure

Software tests also cover:

- CLI validation
- report rendering
- CSV and Markdown escaping
- deterministic scan order
- retry behavior
- progress calculation and rendering
- CSV report file creation

## Boundary

This is not physical validation.

Field-readiness still requires the physical validation runbook in
`docs/validation/modbus-tcp-physical-validation.md`.
