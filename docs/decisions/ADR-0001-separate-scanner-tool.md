# ADR-0001: Keep mitmi-scan Separate From MITMI

## Status

Accepted.

## Context

MITMI is currently a diagnostic proxy and traffic evidence tool. It observes
traffic that a client already sends through the proxy.

`mitmi-scan` is different: it actively probes a device by sending read requests
over a configured address range. Even though the requests are read-only, this is
active device interaction and has different safety, timing, and operator
expectations.

## Decision

Build `mitmi-scan` as a separate console tool in its own repository.

Do not merge scanner behavior into the MITMI proxy runtime for v0.1.

Reuse design ideas from MITMI where they are clearly helpful:

- Zero-based PDU addresses as canonical.
- Field-oriented Markdown output.
- Explicit validation before network activity.
- Clear safety boundaries.

Do not force shared libraries before duplication is proven.

## Consequences

- The scanner can evolve without changing MITMI's proxy guarantees.
- MITMI remains focused on passive in-path diagnosis.
- `mitmi-scan` can make active-probing trade-offs explicit.
- Some concepts may be duplicated initially, which is acceptable while the
  projects are still learning their shapes.
