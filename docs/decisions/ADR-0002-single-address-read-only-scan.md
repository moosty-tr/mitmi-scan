# ADR-0002: Use Read-Only Single-Address Scanning For v0.1

## Status

Accepted.

## Context

Modbus devices often reject a read request when any address in the requested
range is invalid. That means a range read can hide valid addresses that happen
to sit near invalid ones.

The goal of `mitmi-scan` v0.1 is trustworthy address discovery, not maximum scan
speed.

## Decision

The v0.1 scanner will read exactly one address per request.

Supported read-only functions:

- Read Coils, function 01.
- Read Discrete Inputs, function 02.
- Read Holding Registers, function 03.
- Read Input Registers, function 04.

The scanner will not perform writes in v0.1.

Adaptive range scanning, batching, and parallel workers are deferred.

## Consequences

- Scans are slower than range-based probing.
- Results are easier to interpret.
- A valid address is less likely to be hidden by a neighboring invalid address.
- Timeout, retry, delay, and progress behavior become more important because
  scan durations can be long.
- Later adaptive scanning can be added as a separate strategy without changing
  the canonical result model.
