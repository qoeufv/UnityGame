# ADR-001: Separate Flight and Surface Scenes

**Status:** Accepted  
**Date:** 2026-10-09  
**Owners:** Technical lead and game director

## Context

The project supports third-person space flight and first-person planet-surface exploration. Seamless travel would require large-world coordinates, streaming, terrain level-of-detail systems, atmosphere transitions and persistent physics across radically different scales. Those systems would delay validation of the core exploration loop.

## Decision

Use separate Unity scenes for local space flight and each explorable planet surface during the current production path. A small travel/session state transfers the selected destination and return context between scenes.

## Consequences

- Flight and surface controls, cameras and lighting remain independently testable.
- Planet content can be developed and loaded separately.
- Build and memory scope remain manageable.
- Scene transitions require explicit state transfer and loading presentation.
- A future seamless solution would need a migration plan rather than a small visual change.

## Alternatives considered

### Seamless planet transition

Deferred because it requires streaming, coordinate precision, atmosphere rendering and substantially more content.

### One persistent scene with enabled/disabled areas

Rejected for the current slice because scene ownership, lighting and cleanup would become harder to reason about while offering little player benefit.

### Separate scenes with no shared state

Rejected because resetting the player to a universal origin after every surface visit weakens continuity and future progression.

