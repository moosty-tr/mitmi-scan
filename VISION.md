# mitmi-scan Vision

## Product Position

`mitmi-scan` is a practical engineering tool for discovering readable Modbus
addresses on an undocumented or poorly documented Modbus TCP device.

MITMI observes existing traffic. `mitmi-scan` actively asks the device what can
be read. Those are related workflows, but they have different safety and runtime
semantics, so the scanner should remain a separate tool for now.

## Primary Question

The first version should answer one field question well:

```text
Which addresses on this Modbus TCP device respond to read requests, and what
values were observed during the scan?
```

## Goals

- Give field engineers a simple CLI for probing Modbus TCP devices.
- Reveal readable coils, discrete inputs, holding registers, and input
  registers.
- Make long scans understandable with progress, elapsed time, and estimated
  remaining time.
- Produce reports that are useful in support tickets, commissioning notes, and
  reverse-engineering sessions.
- Keep scan behavior deterministic and explainable.
- Treat device safety and network politeness as first-class requirements.

## Non-Goals

- Do not write to devices.
- Do not cache values for clients.
- Do not act as a proxy.
- Do not bridge Modbus to MQTT, OPC UA, HTTP, or any other protocol.
- Do not infer complete device semantics from a raw address map.
- Do not hide device exceptions. Exception responses are useful evidence.

## v0.1 Shape

The initial tool should be a console application with one main scan command.

Inputs:

- Target host.
- Target port, defaulting to 502 when not specified.
- Unit ID.
- One or more Modbus tables.
- Start and end address.
- Per-request timeout.
- Delay between requests.
- Retry count for timeout or transport failures.
- Output format.
- Optional output path for file formats.

Scan behavior:

- Read one address per request.
- Use quantity `1` for all first-version read requests.
- Scan addresses in deterministic order.
- Record every address outcome.
- Keep Modbus exception responses separate from timeouts and transport errors.

Outputs:

- Console progress while scanning.
- Console summary after scanning.
- CSV report when requested.
- Markdown table report when requested.

## Supported Tables

The scanner should use the common Modbus names in external CLI and report
surfaces:

| Table | Function | Value Shape |
| --- | --- | --- |
| `coils` | 01 | Boolean bit |
| `discrete-inputs` | 02 | Boolean bit |
| `holding-registers` | 03 | 16-bit register |
| `input-registers` | 04 | 16-bit register |

## Addressing Policy

Zero-based PDU addresses are canonical.

Reports may include one-based or reference-style columns later, but they should
not replace the zero-based address. This keeps reports aligned with actual
Modbus requests and avoids ambiguity between vendor manuals.

## Safety Position

Scanning is active probing. Even read-only requests can be disruptive on weak,
slow, or poorly implemented devices.

The first version should therefore:

- Require an explicit address range.
- Make timeout, delay, and retry behavior visible.
- Avoid parallel request fan-out.
- Avoid writes completely.
- Preserve enough report detail to explain what happened.

## Relationship To MITMI

MITMI is the diagnostic proxy and traffic evidence tool.

`mitmi-scan` is the active discovery tool.

Do not merge the two runtimes prematurely. If both projects later duplicate
stable reporting or Modbus metadata concepts, consider extracting shared code
only after there is evidence that the shared abstraction is worth the coupling.

## Longer-Term Options

Possible later features, after v0.1 is useful:

- Adaptive scan strategy that splits ranges after exceptions.
- Resume from a previous scan report.
- Separate ranges per table.
- Optional one-based and reference-style report columns.
- More detailed latency statistics.
- Known-device profiles.
- JSON report output.

These should not be built before the single-address scanner is working and
validated.
