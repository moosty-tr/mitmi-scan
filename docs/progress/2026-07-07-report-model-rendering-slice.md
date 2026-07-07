# 2026-07-07 Report Model And Rendering Slice

## Scope

Implemented the second software-only slice for `mitmi-scan`.

This slice adds scan request/result models and report renderers for console,
CSV, and Markdown output.

No Modbus library, TCP connection, simulator, or device interaction is included
in this slice.

## Implemented Behavior

- Added concrete Modbus table modeling separate from CLI table selection.
  - `all` remains a CLI selection.
  - Result records use only concrete tables.
- Added scan request modeling with planned probe calculation.
- Added scan probe/result modeling for:
  - readable values
  - Modbus exception responses
  - timeouts
  - transport errors
  - invalid responses
- Added typed readable values for:
  - boolean bit values
  - 16-bit register values
- Added shared report row projection with stable columns:
  - Table
  - Unit ID
  - Zero-based Address
  - Status
  - Value
  - Exception Code
  - Attempts
  - Duration ms
  - Message
- Added console, CSV, and Markdown renderers.
- Added CSV escaping for commas, quotes, and line breaks.
- Added Markdown escaping for pipes, backslashes, and line breaks.

## Validation

Commands run:

```powershell
dotnet build Mitmi.Scan.slnx
dotnet test Mitmi.Scan.slnx --no-build
```

Result:

- Build succeeded with 0 warnings and 0 errors.
- Test suite passed: 29 tests.

## Next Recommended Slice

Implement the Modbus TCP client adapter behind a small interface.

The adapter should still be kept separate from scan orchestration. It should
prove the protocol boundary first:

- read-only functions only
- readable values
- Modbus exception responses
- timeouts
- transport errors
- invalid responses

Simulator-backed tests become appropriate in the next slice.
