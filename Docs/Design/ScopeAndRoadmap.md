# Scope and Roadmap

## Current scope

The current milestone is the **local exploration slice**:

- Desktop controls for third-person space flight.
- A bounded local orbit environment with selectable destinations.
- Landing entry into Azure World.
- First-person walking, looking and jumping on an island blockout.
- A visible docked ship and an explicit return-to-orbit interaction.
- A small structure-survey mission with progress and completion feedback.
- Runtime-generated geometry that can be replaced by authored assets.

## Explicit non-goals for this milestone

These features are deliberately deferred:

- Seamless flight from orbit through atmosphere to the surface.
- Swimming, diving and underwater traversal.
- Large-world streaming and an unbounded galaxy.
- Full economy, inventory, faction diplomacy or civilization simulation.
- Combat, tactical battles and the earlier Rogue expedition flow.
- Persistent save data for discoveries and flight telemetry.
- Final art direction, production materials, detailed vegetation and ambient wildlife.

Deferring these features protects the current validation target: whether travelling to and exploring one world feels clear and enjoyable.

## Recommended delivery sequence

### Phase 1 — Stabilize the reference slice

Validate movement, camera, collision, landing prompts, surface objectives and ship return on Azure World. Remove placeholder objects that confuse the player before adding more content.

### Phase 2 — Improve environmental readability

Replace the island blockout with authored terrain, shore materials, water motion, lighting, sky treatment, sound and a small number of deliberate environmental props. Keep the mission and interaction contracts stable.

### Phase 3 — Add travel depth

Preserve approach context when returning to orbit, add a second local destination with a fly-by identity, and introduce a small star-system or sector selection view. The transition should communicate distance and destination choice without requiring a full seamless universe.

### Phase 4 — Expand planet variety

Add planets one at a time using the content principles. Each new world must have a unique reason to visit, one readable surface activity and a reliable return path before the next world is started.

### Phase 5 — Persistence and long-term progression

Add save data for surveyed worlds, discovered signals, player route history and destination state only after the local loop and content production workflow are stable.

## Change control

Every proposed feature should answer three questions before implementation:

1. Which player-facing state does it improve?
2. What is the smallest testable version?
3. Does it add a new dependency on final art, persistence or large-world technology?

