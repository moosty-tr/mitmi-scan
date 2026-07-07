# 2026-07-07 Teletek Fire Alarm Panel Physical Validation

## Scope

`mitmi-scan` was run against an approved Teletek fire alarm panel over Modbus
TCP. The validation used read-only input-register scans and zero-based PDU
addresses. Device network details, operator details, and site details are not
stored in this repository.

## Build Under Test

- Scanner commit before this validation note: `6c938ea`
- Output format: compact Markdown
- Table: input-registers
- Modbus function: 04, read input registers
- Writes performed: none

## Observed Results

| Requested Address Range | Active Addresses Found | Report Summary |
| --- | --- | --- |
| 100..500 | 500 | Scanned 401 input registers in 96.286 seconds; found total of 1 active register. |
| 1000..1200 | 1000..1200 | Scanned 201 input registers in 48.297 seconds; found total of 201 active registers. |

## Notes

- The compact Markdown report listed successful reads only, which matched the
  intended field workflow for identifying readable address ranges.
- Values were rendered as hex, decimal, ASCII, and binary.
- A prior verbose local report included Modbus exception rows and long library
  exception messages. That generated report was reviewed but intentionally not
  committed because it is less useful as durable validation evidence.
- No device instability or unexpected operator-visible side effects were
  reported after the scan.

## Result

Pass. The scanner successfully revealed readable input-register addresses on a
physical Teletek fire alarm panel using the v0.1 read-only Modbus TCP workflow.
