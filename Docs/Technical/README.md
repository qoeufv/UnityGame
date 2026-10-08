# Technical Documentation

This directory is the technical source of truth for the current Game Frontier prototype. It describes what the project does today, the boundaries between runtime systems, and the seams that should be preserved while the prototype becomes a production game.

## Documents

| Document | Purpose |
| --- | --- |
| [Runtime Architecture](RuntimeArchitecture.md) | Scene ownership, runtime generation, flight-to-surface flow, state/data flow, input and camera boundaries, and interaction contracts. |
| [Asset and Scene Pipeline](AssetAndScenePipeline.md) | Unity scene/bootstrap conventions, imported model and texture rules, generated content ownership, and build registration. |
| [Technical Debt and Extension Plan](TechnicalDebtAndExtensionPlan.md) | Known limitations, risk order, and recommended interfaces for persistence, streaming, landing, missions, and future planets. |

## Scope and status

These documents describe the implementation available in Unity `6000.6.0f1` as of 2026-10-09. The flight and surface scenes are runtime-generated prototypes. They are deliberately small and testable, but they are not yet a complete production content pipeline.

When a design decision changes, update the relevant document in the same change as the code. Keep the wording explicit about whether a statement is **Implemented**, **Planned**, or **Deferred**.

## Source of truth

- Runtime code: `Assets/Scripts/Flight/`
- Editor setup and validation: `Assets/Editor/`
- Planet data: `Assets/Resources/Planets/`
- Imported Azure World kit: `Assets/Art/Planets/AzureWorld/Imported/`
- Scene assets: `Assets/Scenes/`
- Player-facing prototype notes: `Docs/SpaceFlightPrototype.md`
- Weekly delivery reports: `Docs/Week*Progress.md`

## Change review checklist

Before merging a technical change, reviewers should check:

1. Does the change preserve the boundary between flight, surface traversal, presentation, and data?
2. Does it work when runtime-generated objects are rebuilt from an empty scene?
3. Are imported assets referenced through serialized fields or an explicit setup tool rather than fragile scene-only paths?
4. Are colliders, scan targets, input, and camera behavior documented when their contract changes?
5. Is a validation command or manual verification step recorded in the relevant report?
