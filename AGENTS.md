# AGENTS.md

## Working Posture

Do not optimize for writing code quickly.

Optimize for software that will still be maintainable after five years.

If an idea introduces unnecessary complexity, say so directly. Prefer the
simpler solution when it is technically sufficient, and explain the trade-off.

Assume the user is an experienced software engineer. Technical discussion is
preferred over simplified explanations.

## Project-Specific Guidance

`mitmi-scan` is a read-only Modbus TCP address scanner. It is related to MITMI's
diagnostic and reverse-engineering workflow, but it is not the MITMI proxy.

Keep the first implementation narrow:

- Single-address scanning only.
- Read-only Modbus functions only.
- Modbus TCP only.
- Console CLI only.
- Zero-based PDU addresses as the canonical address representation.
- Console, CSV, and Markdown reports only.

Do not add these to the first implementation:

- Adaptive range scanning.
- Writes.
- Caching.
- Proxy behavior.
- Multiple concurrent clients.
- MQTT, OPC UA, HTTP, or other protocol conversion.
- A dashboard or long-running service mode.

## Planning And Implementation

Stay in planning mode until the user explicitly asks for implementation.

When implementation starts, prefer small, verifiable slices. Each slice should
leave the CLI usable or at least buildable, with tests appropriate to the risk.

Software-only tests can prove argument validation, report formatting, progress
calculation, scan orchestration, and protocol behavior against a simulator.
Field-readiness still requires validation against an approved Modbus device or
bench rig because scanning actively probes device addresses.

## Safety Boundary

The scanner must not perform writes in v0.1. All first-version behavior should
use Modbus read functions only:

- Function 01: read coils.
- Function 02: read discrete inputs.
- Function 03: read holding registers.
- Function 04: read input registers.

Modbus exception responses are scan results, not necessarily failures. Timeouts,
connection failures, and malformed responses should be recorded distinctly from
valid exception responses.
