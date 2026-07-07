# ADR-0003: Use Zero-Based Addresses As The Canonical Report Key

## Status

Accepted.

## Context

Modbus documentation often mixes address notations:

- Zero-based PDU addresses.
- One-based addresses.
- Reference-style labels such as 40001.
- Vendor-specific table offsets.

Mixing these in the scanner's internal model would make reports ambiguous.

## Decision

`mitmi-scan` will use zero-based PDU addresses as the canonical address in scan
requests, result records, CSV output, and Markdown output.

Future reports may add optional one-based or reference-style columns, but those
columns must not replace the zero-based address.

## Consequences

- Reports align with the actual Modbus request address.
- Users comparing vendor manuals may need optional display columns later.
- The scanner avoids guessing which vendor notation is intended.
- This keeps `mitmi-scan` aligned with MITMI's existing discovery-report
  direction.
