# Milestone Acceptance

This document defines the evidence required to move between production stages. A milestone is accepted only when its player-facing flow, technical health and documentation agree.

## Prototype acceptance

- Core movement and camera can be evaluated.
- One complete gameplay loop exists, even with placeholder art.
- Major technical risks are identified.
- The project opens and compiles on the primary development machine.
- Known limitations are documented.

## Vertical slice acceptance

- A new player can complete space approach, landing, surface objective and return to orbit.
- Azure World has coherent visual scale, terrain, ocean and structure placement.
- Player input, collision, scanning and scene transitions pass the regression checklist.
- Mission state survives the required scene transitions.
- HUD communicates objectives and contextual actions.
- Windows and macOS standalone development builds launch successfully.
- Representative art, audio and UI quality is close to the intended final direction.
- Performance targets are measured and recorded.

## Alpha acceptance

- All primary systems are implemented.
- The planned content path is playable from start to finish.
- Save/load and settings are reliable.
- No known blocker or critical data-loss issue remains.
- Performance is within the defined target on minimum hardware.
- Asset provenance and licenses are recorded.

## Beta acceptance

- Content is feature-complete and locked except for fixes.
- Full regression is complete on target platforms.
- Accessibility, controls and display settings have been reviewed.
- Release packaging, versioning and upgrade behavior have been tested.
- Known issues are triaged and player-facing notes are prepared.

## Release acceptance

- Release candidate commit and Git tag are fixed.
- Windows and macOS packages are generated from that commit.
- Smoke tests pass on clean machines.
- Changelog, credits, licenses and known issues are included.
- GitHub Release artifacts and checksums are prepared.
- Rollback and hotfix ownership are assigned.

## Current milestone

The project is currently between Prototype and Vertical Slice. The immediate acceptance target is a stable Azure World loop with a standalone build that can be tested by someone without Unity.

