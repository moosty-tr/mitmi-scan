# mitmi-scan Project Memory

Created: 2026-07-07

## Why This Repo Exists

`mitmi-scan` is a new repo for a Modbus TCP address scanner. It complements
MITMI but should not be merged into the MITMI proxy runtime.

MITMI observes existing client/device traffic. `mitmi-scan` actively probes a
device to reveal readable address maps.

## Accepted Initial Scope

- Console application.
- Modbus TCP only.
- Single-address scanning only.
- Read-only functions only.
- Tables:
  - holding registers
  - input registers
  - discrete inputs
  - coils
- Configurable:
  - unit ID
  - start address
  - end address
  - timeout
  - delay
  - retries
- Progress:
  - ASCII progress bar
  - estimated time remaining
- Reports:
  - console
  - CSV
  - Markdown

## Important Design Constraints

- Zero-based PDU address is canonical.
- Require explicit scan ranges.
- Do not perform writes in v0.1.
- Do not implement adaptive range scanning in v0.1.
- Do not add cache/proxy behavior.
- Do not add MQTT/OPC/protocol conversion.
- Treat Modbus exception responses as scan evidence, not process failures.
- Distinguish exceptions, timeouts, transport errors, and invalid responses.

## Preferred Working Style

The user prefers technical design discussion and maintainability over fast code.

Challenge unnecessary complexity. If a simpler solution is better, explain why.

Stay in planning mode until implementation is explicitly requested.

## Next Session Start

Read these first:

- `AGENTS.md`
- `README.md`
- `VISION.md`
- `docs/progress/2026-07-07-fresh-session-handoff.md`
- `docs/planning/v0.1-implementation-plan.md`
- `docs/architecture/modbus-address-scanner-design.md`
- `docs/decisions/`

Recommended next discussion:

- Confirm CLI option names.
- Confirm whether `--table all` should scan the same address range across all
  four tables.
- Confirm default timeout, delay, and retry values.
- Confirm whether CSV/Markdown write to stdout when `--output` is omitted or
  require an explicit path.
