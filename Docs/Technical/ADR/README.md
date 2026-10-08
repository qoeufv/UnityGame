# Architecture Decision Records

Architecture Decision Records capture technical choices that affect multiple systems or constrain future work. They explain why a decision was made, not only what the code currently does.

## Status values

- **Proposed:** under review.
- **Accepted:** approved and currently applicable.
- **Superseded:** replaced by another ADR.
- **Deprecated:** retained for history but should no longer guide new work.

## File naming

Use `ADR-NNN-Short-Title.md`. Numbers are never reused. A replacing decision links to the earlier ADR and changes its status to Superseded.

## Record template

```text
# ADR-NNN: Decision title

Status: Proposed
Date: YYYY-MM-DD
Owners: role or person

## Context
What problem or constraint requires a decision?

## Decision
What approach will the project use?

## Consequences
What becomes easier, harder or constrained?

## Alternatives considered
What other options were evaluated and why were they rejected?
```

## Current records

| ADR | Status | Decision |
| --- | --- | --- |
| [ADR-001](ADR-001-Separate-Flight-And-Surface-Scenes.md) | Accepted | Use separate flight and planet-surface scenes for the current production path |

