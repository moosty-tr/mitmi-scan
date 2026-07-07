# Modbus Address Scanner Design

## Status

Planning draft.

## Design Summary

`mitmi-scan` should perform deterministic, read-only, single-address Modbus TCP
scans. It should prioritize correctness, explainability, and field usefulness
over scan speed.

The first version deliberately avoids adaptive range scanning. Adaptive probing
can be valuable later, but it adds strategy complexity before the result model,
reports, and timeout behavior have been proven.

## Runtime Flow

1. Parse CLI options.
2. Validate target, table selection, address range, timeout, delay, retries, and
   output settings.
3. Calculate the total number of address probes.
4. Connect to the Modbus TCP target.
5. For each selected table and address:
   - Send exactly one read request for quantity `1`.
   - Record the response, exception, timeout, or transport failure.
   - Apply retry policy only to timeout or transport failure outcomes.
   - Apply the configured delay before the next address.
   - Update progress and ETA.
6. Write the final report.
7. Exit with a stable code.

## Scan Strategy

The only v0.1 scan strategy is `single`.

For each address, the scanner sends a request whose quantity is exactly `1`.
This avoids the common discovery problem where a valid address inside a larger
range cannot be distinguished because the device rejects the entire range.

Trade-off:

- The scan is slower.
- The report is easier to trust.
- Device behavior is easier to explain.
- Later adaptive scanning can be added without invalidating the result model.

## Table Handling

Supported tables:

| CLI Name | Modbus Function | Request Quantity |
| --- | --- | --- |
| `coils` | Read Coils, function 01 | 1 bit |
| `discrete-inputs` | Read Discrete Inputs, function 02 | 1 bit |
| `holding-registers` | Read Holding Registers, function 03 | 1 register |
| `input-registers` | Read Input Registers, function 04 | 1 register |

The scanner should not use write functions in v0.1.

## Connection Policy

Prefer a persistent TCP connection for the scan. Reconnecting for every address
would make long scans slower and can create unnecessary load on weak devices.

If a timeout or transport error leaves the connection state uncertain, reconnect
before retrying or continuing.

Modbus exception responses are valid protocol responses and should not force a
reconnect by themselves.

## Result Model

Each address probe should produce one durable result record.

Recommended fields:

- Target host and port.
- Unit ID.
- Table.
- Zero-based address.
- Status:
  - `readable`
  - `modbus-exception`
  - `timeout`
  - `transport-error`
  - `invalid-response`
- Value when readable.
- Modbus exception code when present.
- Attempt count.
- Duration in milliseconds.
- Timestamp in UTC.
- Optional diagnostic message.

This model should drive console, CSV, and Markdown output. Avoid making each
output format invent its own interpretation of scan outcomes.

## Progress And ETA

Progress should be based on completed address probes divided by total planned
probes.

For interactive consoles, update one progress line in place when practical:

```text
[##########----------] 50.0% 500/1000 ETA 00:01:42
```

When output is redirected or not interactive, prefer periodic line-oriented
progress so logs remain readable.

ETA should use observed average duration rather than assuming every request
takes the configured timeout.

## Report Formats

Console output should optimize for quick field inspection.

CSV output should optimize for spreadsheet import and later tooling.

Markdown output should optimize for support handoff and documentation.

Recommended common columns:

- Table.
- Unit ID.
- Zero-based address.
- Status.
- Value.
- Exception code.
- Attempts.
- Duration ms.
- Message.

Readable values and Modbus exceptions should both appear in reports. Hiding
exceptions would remove useful map information.

## Exit Codes

Recommended initial exit-code policy:

- `0`: Scan completed, even if some addresses returned exceptions or timeouts.
- `1`: Invalid CLI options.
- `2`: Target could not be reached before scanning started.
- `3`: Scan aborted by cancellation or unrecoverable runtime error.
- `4`: Report output failed.

Do not treat normal Modbus exception responses as process failures.

## Physical Validation Boundary

Simulator tests are necessary but not sufficient for field-readiness claims.

Before calling the tool field-ready, validate against an approved Modbus TCP
device or bench rig. Start with a small range, nonzero delay, and no retries.
