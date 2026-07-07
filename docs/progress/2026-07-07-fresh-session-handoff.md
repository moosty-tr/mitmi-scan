# 2026-07-07 Fresh Session Handoff

## Purpose

This repo was created for `mitmi-scan`, a separate Modbus TCP address scanner
tool related to the MITMI diagnostic workflow.

The next session should start from this handoff, not from chat memory alone.

## User-Accepted Direction

The scanner should be simple first:

- Build `mitmi-scan` as an additional tool in this repo.
- Use single-address scanning only.
- Support all four common read tables:
  - holding registers
  - input registers
  - discrete inputs
  - coils
- Make unit ID, start/end address, timeout, delay, and retries configurable.
- Show estimated time remaining and an ASCII progress bar.
- Output reports as console, CSV, or Markdown.

## Current Repo State

At handoff time, this repo contains planning and decision documents only.

No implementation code has been added.

Read these files first:

1. `AGENTS.md`
2. `README.md`
3. `VISION.md`
4. `docs/architecture/modbus-address-scanner-design.md`
5. `docs/planning/v0.1-implementation-plan.md`
6. `docs/decisions/ADR-0001-separate-scanner-tool.md`
7. `docs/decisions/ADR-0002-single-address-read-only-scan.md`
8. `docs/decisions/ADR-0003-addressing-and-reporting.md`
9. `.codex/PROJECT_MEMORY.md`

## Suggested Next Step

Begin with a short design review of the CLI contract and project shape before
creating code.

Recommended first implementation slice, once approved:

1. Create a .NET solution and one console project.
2. Add CLI parsing and validation.
3. Validate host, port, unit ID, table, start/end range, timeout, delay,
   retries, output format, and output path.
4. Print a dry scan plan.
5. Add tests for validation.

Do not open network connections in the first slice.

## Physical-Test Boundary

No physical test is needed for the planning scaffold.

Simulator-backed tests can validate scan orchestration and protocol behavior,
but field-readiness requires an approved Modbus TCP device, gateway, PLC
simulator connected through real network hardware, or bench rig.

Because `mitmi-scan` actively probes device addresses, use read-only functions,
small ranges, nonzero delay, and no retries for first hardware contact.
